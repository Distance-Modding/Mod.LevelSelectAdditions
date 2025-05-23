using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Distance.LevelSelectAdditions.Helpers
{
	/// <summary>
	/// Optimized and organized reflection for use with Steamworks,
	/// which is needed due to the lack of <c>Assembly-CSharp-firstpass.dll</c> dependency.
	/// <para/>
	/// <see cref="SteamworksHelper.Init()"/> must be called before using.
	/// </summary>
	public static class SteamworksHelper
	{
		public const int VoteIndex_For = 0;
		public const int VoteIndex_Against = 1;
		public const int VoteIndex_None = 2;


		//private static Assembly assembly_FirstPass;

		public static void Init()
		{
			// Nothing here yet...

			//assembly_FirstPass = typeof(SteamworksLeaderboard.Leaderboard).GetField("steamHandle_").FieldType.Assembly;
		}

		// WIP:
		/*private static Expression ParseMethodAccessor(Expression body, string member)
		{
			string method = member.Substring(0, member.Length - 2); // Remove "()"
			int sepIndex = method.LastIndexOf("::");
			if (sepIndex != -1)
			{
				string classPath = method.Substring(0, sepIndex);
				method = method.Substring(sepIndex + 2);
				classPath = classPath.Replace("::", ".");

				Type staticType = typeof(GameManager).Assembly.GetType(classPath, false);
				if (staticType == null)
				{
					staticType = assembly_FirstPass.GetType(classPath, false);
				}
				if (staticType == null)
				{
					throw new ArgumentException($"Could not find declaring type for \"{member}\"");
				}
				return Expression.Call(staticType, method, null, body);
			}
			else
			{
				return Expression.Call(body, method, null);
			}
		}*/

		// Using this allows for compiling expressions that may need to access non-public fields or properties.
		// see: <https://stackoverflow.com/a/16208620/7517185>
		private static Delegate CreateAccessor(Type type, string accessorPath)
		{
			Mod.Log.LogDebug($"CreateAccessor: {type.Name}.{accessorPath}");
			Stopwatch watch = Stopwatch.StartNew();

			ParameterExpression param = Expression.Parameter(type, "x");
			Expression body = param;
			foreach (string member in accessorPath.Split('.'))
			{
				// WIP:
				//if (member.EndsWith("()"))
				//{
				//	body = ParseMethodAccessor(body, member);
				//}
				//else
				//{
					body = Expression.PropertyOrField(body, member);
				//}
			}
			Delegate compiled = Expression.Lambda(body, param).Compile();

			watch.Stop();
			Mod.Log.LogDebug($"CreateAccessor: {watch.ElapsedMilliseconds}ms");
			return compiled;
		}

		private static Func<T, R> CreateAccessor<T, R>(string accessorPath)
		{
			return (Func<T, R>)CreateAccessor(typeof(T), accessorPath);
		}
	}
}
