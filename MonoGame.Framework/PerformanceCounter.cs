// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 

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
		
		public override string ToString ()
		{
			return string.Format("[{0}({1}%)\t HitCount={2}\t TotalTime={3}ms\t MaxTime={4}ms\t AverageTime={5}ms]", Name,(100*TotalTime)/PerformanceCounter.ElapsedTime,HitCount,TotalTime, MaxTime, TotalTime/HitCount);
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
		private static long _startTime = DateTime.Now.Ticks;
		private static long _endTime;
		
		public static void Dump()
		{
            _endTime = DateTime.Now.Ticks;

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
            _startTime = DateTime.Now.Ticks;
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
                item.PreviousTime = DateTime.Now.Ticks;		
			}
			else 
			{
				item = new PerformanceItem();
#if !WINRT
    			var stackTrace = new StackTrace();
    			var stackFrame = stackTrace.GetFrame(1);
    			MethodBase methodBase = stackFrame.GetMethod();

				item.Name = "ID: " + Name+" In " + methodBase.ReflectedType.ToString()+"::"+methodBase.Name;
#else
                item.Name = "ID: " + Name;
#endif
                item.PreviousTime = DateTime.Now.Ticks;
                _list.Add(Name,item);
			}			
		}
		
		public static void EndMensure(string Name)
		{
			PerformanceItem item = _list[Name];
            var elapsedTime = DateTime.Now.Ticks - item.PreviousTime;
			if (item.MaxTime < elapsedTime) 
			{
				item.MaxTime = elapsedTime;
			}
			item.TotalTime += elapsedTime;
			item.HitCount ++;
		}	}
}

#endif