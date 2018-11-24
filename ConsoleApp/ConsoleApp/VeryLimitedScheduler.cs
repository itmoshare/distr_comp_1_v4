using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp
{
	/// <summary>
	///     Scheduler which will use one less than the number of cores on the computer to
	///     perform asynchronous tasks.
	///     Do not use this scheduler if the tasks would block while new tasks would be added
	/// </summary>
	public class VeryLimitedScheduler : TaskScheduler, IDisposable
	{
		private readonly Thread[] _pool;
		private readonly TimeSpan _takeFrequency = TimeSpan.FromSeconds(30);
		private readonly CountdownEvent _threadsExited;
		private BlockingCollection<Task> _queue;

		public VeryLimitedScheduler(int numberOfThreads)
		{
			_queue = new BlockingCollection<Task>();
			if (0 >= numberOfThreads) return;
			_threadsExited = new CountdownEvent(numberOfThreads);
			_pool = new Thread[numberOfThreads];
			ThreadStart ts = RunPool;
			for (var core = 0; core < _pool.Length; core++)
			{
				_pool[core] = new Thread(ts);
				_pool[core].Start();
			}
		}

		public override int MaximumConcurrencyLevel => _pool.Length + 1;

		public void Dispose()
		{
			if (null != _queue)
			{
				_queue.CompleteAdding();
				if (null != _threadsExited)
				{
					_threadsExited.Wait(_takeFrequency);
					_threadsExited.Dispose();
				}

				_queue.Dispose();
				_queue = null;
			}
		}

		private void RunPool()
		{
			Thread.CurrentThread.IsBackground = true;
			try
			{
				while (!_queue.IsCompleted)
					if (_queue.TryTake(out var t, _takeFrequency))
						TryExecuteTask(t);
			}
			finally
			{
				_threadsExited.Signal();
			}
		}

		protected override IEnumerable<Task> GetScheduledTasks()
		{
			return _queue.ToList();
		}

		protected override void QueueTask(Task task)
		{
			_queue.Add(task);
		}

		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{
			if (!taskWasPreviouslyQueued) return TryExecuteTask(task);

			return false;
		}
	}
}