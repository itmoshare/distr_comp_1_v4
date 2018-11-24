for ($i=1; $i -le 32; $i++)
{
  Start-Process -NoNewWindow -Wait dotnet -ArgumentList ".\ConsoleApp.dll -p a.txt -t $i"
}