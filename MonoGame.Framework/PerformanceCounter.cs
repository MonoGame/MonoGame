// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.Xna.Framework
{

	internal class PerformanceItem
	{		
		public void Dump()
		{
			Debug.WriteLine(ToString());
		}
		
		public override string ToString()
		{
            return "[" + Name + "(" + (100 * TotalTime) / PerformanceCounter.ElapsedTime + "%)\t HitCount=" + HitCount + "\t TotalTime=" + TotalTime + "ms\t MaxTime=" + MaxTime + "ms\t AverageTime=" + TotalTime / HitCount + "ms]";
		}

		public long PreviousTime {get;set;}
		public long TotalTime {get;set;}
		public long MaxTime {get;set;}
		public long HitCount {get;set;}
		public string Name {get;set;}
	}
	
	public static class PerformanceCounter
	{
		private static Dictionary<string,PerformanceItem> _list = new Dictionary<string, PerformanceItem>();
		private static long _startTime = DateTime.UtcNow.Ticks;
		private static long _endTime;
		
		public static void Dump()
		{
            _endTime = DateTime.UtcNow.Ticks;

            Debug.WriteLine("Performance count results");
            Debug.WriteLine("=========================");
            Debug.WriteLine("Execution Time: " + ElapsedTime + "ms.");
			
			foreach (PerformanceItem item in _list.Values)
			{
				item.Dump();
			}
			
			Debug.WriteLine("=========================");
		}
		
		public static void Begin()
		{
            _startTime = DateTime.UtcNow.Ticks;
		}
				
		public static long ElapsedTime
		{
			get 
			{
				return _endTime-_startTime;
			}
		}
		
		public static void BeginMensure(string Name)
		{			
			PerformanceItem item;
			if (_list.ContainsKey(Name))
			{
				item = _list[Name];
                item.PreviousTime = DateTime.UtcNow.Ticks;		
			}
			else 
			{
				item = new PerformanceItem();
#if !WINRT
    			var stackTrace = new StackTrace();
    			var stackFrame = stackTrace.GetFrame(1);
    			MethodBase methodBase = stackFrame.GetMethod();

				item.Name = "ID: " + Name+" In " + methodBase.ReflectedType.ToString() + "::" + methodBase.Name;
#else
                item.Name = "ID: " + Name;
#endif
                item.PreviousTime = DateTime.UtcNow.Ticks;
                _list.Add(Name,item);
			}			
		}
		
		public static void EndMensure(string Name)
		{
			PerformanceItem item = _list[Name];
            var elapsedTime = DateTime.UtcNow.Ticks - item.PreviousTime;
			if (item.MaxTime < elapsedTime) 
			{
				item.MaxTime = elapsedTime;
			}
			item.TotalTime += elapsedTime;
			item.HitCount ++;
		}	}
}

#endif