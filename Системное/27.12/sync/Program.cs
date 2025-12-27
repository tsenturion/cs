Monitor.Enter(locker);
try
{
	// критическая секция
}
finally
{
	Monitor.Exit(locker);
}

private static readonly object locker = new object();

class Counter
{
	private int _value = 0;
	private readonly object _locker = new object();

	public void Increment()
	{
		lock (_locker)
		{
			_value++;
		}
	}

	public int GetValue()
	{
		lock (_locker)
		{
			return _value;
		}
	}
}

lock (this)
{
	_value++;
}

lock ("myLock")
{
	_value++;
}


object locker = new object();

lock (locker)
{
	// ...
}

locker = new object();


lock (_locker)
{
	DoWork();
	CallService();
	WriteToFile();
}





lock (locker1)
{
	lock (locker2)
	{
		// ...
	}
}

lock (locker2)
{
	lock (locker1)
	{
		// ...
	}
}


lock (_locker)
{
	Thread.Sleep(1000);
}
