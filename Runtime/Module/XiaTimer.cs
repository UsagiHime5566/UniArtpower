namespace HimeLib
{
	using System;
	using System.Collections;
	using UnityEngine;

	/// <summary>
	/// Ready to use timers for coroutines
	/// </summary>
	/// <summary>
	/// Ready to use timers for coroutines
	/// </summary>
	public class XiaTimer
	{
		/// <summary>
		/// 簡單的正計時器, 可以指定每次停頓時執行的動作
		/// </summary>
		/// <param name="duration">停頓的間隔</param>
		/// <param name="callback">停頓時調用的函數</param>
		/// <returns></returns>
		public static IEnumerator Start(float duration, Action callback)
		{
			return Start(duration, false, callback);
		}


		/// <summary>
		/// 簡單的正計時器, 可以指定每次停頓時執行的動作
		/// </summary>
		/// <param name="duration">每次停頓的間隔</param>
		/// <param name="repeat">是否重覆執行</param>
		/// <param name="callback">停頓時調用的函數</param>
		/// <returns></returns>
		public static IEnumerator Start(float duration, bool repeat, Action callback)
		{
			do
			{
				yield return new WaitForSeconds(duration);

				if (callback != null)
					callback();

			} while (repeat);
		}

		public static IEnumerator StartRealtime(float time, System.Action callback)
		{
			float start = Time.realtimeSinceStartup;
			while (Time.realtimeSinceStartup < start + time)
			{
				yield return null;
			}

			if (callback != null) callback();
		}

		public static IEnumerator NextFrame(Action callback)
		{
			yield return new WaitForEndOfFrame();

			if (callback != null)
				callback();
		}
	}

    //from : liusuwanxia.Timer
}