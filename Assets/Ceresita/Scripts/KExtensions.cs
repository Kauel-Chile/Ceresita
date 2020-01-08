using UnityEngine;

using System.Collections;

using System.Collections.Generic;

using System.Threading;

using System;



using System.Linq;

using System.Linq.Expressions;



using System.IO;

using System.Text;

using System.Text.RegularExpressions;



using System.Security.Cryptography;

namespace K {
    namespace Extensions {


public static class KExtensions {



    public static void SetPivot(this RectTransform rectTransform, Vector2 pivot) {

        if (rectTransform == null) return;



        Vector2 size = rectTransform.rect.size;

        Vector2 deltaPivot = rectTransform.pivot - pivot;

        Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);

        rectTransform.pivot = pivot;

        rectTransform.localPosition -= deltaPosition;

    }



    public delegate void ActionWithIndex(int x);

	public struct ActionWithIndexData {

		public ActionWithIndex TheAction;

		public Action TheReport;

		public Action TheFinal;

		public int index;

		public int indexstart;

		public int indexend;

		public int indexReportEvery;



		public ActionWithIndexData(ActionWithIndex aAction,int aIndex, int aIndexStart, int aIndexEnd,Action aReport,Action aFinal, int aReportEvery){

			TheAction = aAction;

			TheReport = aReport;

			TheFinal = aFinal;

			index = aIndex;

			indexstart = aIndexStart;

			indexend = aIndexEnd;

			indexReportEvery = aReportEvery;

		}

	}



	private static object FunctionsProgressDataLock = new object ();

	private static int FunctionsRunning = 0;

	private static int FunctionsExecuted = 0;



	public static int GetFunctionsExecuted(){

		int result = 0;

		lock (FunctionsProgressDataLock) result = FunctionsExecuted;

		return result;

	}



	public static int GetFunctionsRunning(){

		int result = 0;

		lock (FunctionsProgressDataLock) result = FunctionsRunning;

		return result;

	}



	/// <summary>

	///		Ejecuta una funcion del tipo void f(long index) muchas veces en hilos paralelos, encola acciones a intervalos y al final

	/// las acciones encoladas se pueden ejecutar en el hilo principal usando ExecuteQueuedActions()

	/// </summary>

	//this.ExecuteALotOfFunctionsAndQueueProgressAndEnd (DoStuff, Progress, Final, 10, 100, 2, 10);

	public static void ExecuteALotOfFunctionsAndQueueProgressAndEnd(this MonoBehaviour MB,ActionWithIndex TheAction,Action Progress,Action Final,int StartIndex = 0,int EndIndex = 100, int ParallelCount = 8,int ProgressEvery = 10){

		lock (FunctionsProgressDataLock) {

			FunctionsRunning = 0;

			FunctionsExecuted = 0;

		}



        int i = StartIndex;

		while(i <= EndIndex){

			lock (FunctionsProgressDataLock) {

				if (FunctionsRunning < ParallelCount) {

					FunctionsRunning++;

					ExecuteAndQueueProgressAndEnd (MB, new ActionWithIndexData(TheAction,i,StartIndex,EndIndex,Progress,Final,ProgressEvery));

					i++;

				}

			}

			Thread.Sleep (1);

		}

	}







	/// <summary>

	///		Lista de las Acciones Encoladas. Se ejecutan generalmente en el hilo principal usando ExecuteQueuedActions();

	/// </summary>

    public static List<Action> QueuedActions = new List<Action>();



    public static void ExecuteQueuedActions(this MonoBehaviour MB){

        lock (QueuedActions) { 

            for (int i = 0; i < QueuedActions.Count; i++){

                Action A = QueuedActions[i];

				if(A!=null) A.Invoke();

            }

            QueuedActions.Clear();

        }

    }



    /// <summary>

    /// 	Round to int

    /// </summary>

    public static int ToInt(this float f){

        return Mathf.RoundToInt(f);

    }



    /// <summary>

    /// 	Round to byte

    /// </summary>

    public static byte ToByte(this float f){

        return (byte)Mathf.RoundToInt(f);

    }



    /// <summary>

    ///     Control float to reach the Target Value

    /// </summary>

    /// <param name="Target"> The Target Value</param>

    /// <param name="deltaTime"> Delta Time in seconds</param>

    /// <param name="speed"> Speed in Units/Secounds</param>

    public static float Control(this float Variable, float Target, float deltaTime, float speed = 1.0f) {

        float error = Target - Variable;

        float action = Mathf.Clamp01(deltaTime * speed);

        return Variable + error * action;

    }





    // MONOBEHAVIOUR EXTENSIONS

    /// <summary>

    ///     Create a Clone of a Prefab

    /// </summary>

    /// <param name="prefab"> The prefab to clone</param>

    /// <param name="parent"> The parent of the new object</param>

    /// <param name="LocalPos"> The local position</param>

    /// <param name="LocalScale"> The local scale</param>

    /// <returns></returns>

    public static GameObject InstantiatePrefab(this MonoBehaviour MB,GameObject prefab, Transform parent, Vector3 LocalPos,Vector3 LocalScale) {

        GameObject go = GameObject.Instantiate(prefab);

        Transform t = go.GetComponent<Transform>();

        t.SetParent(parent);

        t.localPosition = LocalPos;

        t.localScale = LocalScale;

        return go;

    }



    //ExecuteInPoolThread

    public static bool ExecuteInPoolThread(this MonoBehaviour MB, Action A){

        return ThreadPool.QueueUserWorkItem(o => A.Invoke());

    }



    //ExecuteInNewThread

    public static Thread ExecuteInNewThread(this MonoBehaviour MB, Action A){

        Thread t = new Thread(o => A.Invoke());

        t.Start();

        return t;

    }





    //Execute ASync and Notify

    public static void ExecuteAndNotifyEnd(this MonoBehaviour MB, Action ASync, Action Final){

        Action[] ASyncAndFinal = new Action[2];

        ASyncAndFinal[0] = ASync;

        ASyncAndFinal[1] = Final;

        ASync.BeginInvoke(new AsyncCallback(NotifyEnd), ASyncAndFinal);

    }

	

    public static void NotifyEnd(IAsyncResult AR){

        Action[] ASyncAndFinal = (Action[])AR.AsyncState;

        ASyncAndFinal[0].EndInvoke(AR);

        ASyncAndFinal[1].Invoke();

    }



	//Execute ASync and Notify

	public static void ExecuteAndQueueProgressAndEnd(this MonoBehaviour MB, ActionWithIndexData ASyncData){

		ActionWithIndex ASync = ASyncData.TheAction;

		ASync.BeginInvoke (ASyncData.index, new AsyncCallback (QueueProgressAndEnd), ASyncData);

	}



	public static void QueueProgressAndEnd(IAsyncResult AR){

		ActionWithIndexData ASyncData = (ActionWithIndexData)AR.AsyncState;

		ASyncData.TheAction.EndInvoke(AR);



		lock (FunctionsProgressDataLock) {

			FunctionsExecuted++;

			FunctionsRunning--;

		}



		lock (QueuedActions) {

			if ((ASyncData.index % ASyncData.indexReportEvery) == 0) QueuedActions.Add(ASyncData.TheReport);

			if (ASyncData.index >= ASyncData.indexend) QueuedActions.Add (ASyncData.TheFinal);

		}

	}



    //Execute ASync and Notify

    public static void ExecuteAndQueueEnd(this MonoBehaviour MB, Action ASync, Action Final){

        Action[] ASyncAndFinal = new Action[2];

        ASyncAndFinal[0] = ASync;

        ASyncAndFinal[1] = Final;

        ASync.BeginInvoke(new AsyncCallback(QueueEnd), ASyncAndFinal);

    }

	

    public static void QueueEnd(IAsyncResult AR){

        Action[] ASyncAndFinal = (Action[])AR.AsyncState;

        ASyncAndFinal[0].EndInvoke(AR);

        lock (QueuedActions) {

            QueuedActions.Add(ASyncAndFinal[1]);

        }

    }



    /// <summary>

    /// 	Timestamp in milliseconds

    /// </summary>

    public static double TimeStamp(this MonoBehaviour MB) {

        return DateTime.Now.TimeOfDay.TotalMilliseconds;

    }



    /// <summary>

    /// 	Adds a value uniquely to to a collection and returns a value whether the value was added or not.

    /// </summary>

    /// <typeparam name = "T">The generic collection value type</typeparam>

    /// <param name = "collection">The collection.</param>

    /// <param name = "value">The value to be added.</param>

    /// <returns>Indicates whether the value was added or not</returns>

    /// <example>

    /// 	<code>

    /// 		list.AddUnique(1); // returns true;

    /// 		list.AddUnique(1); // returns false the second time;

    /// 	</code>

    /// </example>

    public static bool AddUnique<T>(this ICollection<T> collection, T value){

        var alreadyHas = collection.Contains(value);

        if (!alreadyHas){

            collection.Add(value);

        }

        return alreadyHas;

    }



    /// <summary>

    /// 	Adds a range of value uniquely to a collection and returns the amount of values added.

    /// </summary>

    /// <typeparam name = "T">The generic collection value type.</typeparam>

    /// <param name = "collection">The collection.</param>

    /// <param name = "values">The values to be added.</param>

    /// <returns>The amount if values that were added.</returns>

    public static int AddRangeUnique<T>(this ICollection<T> collection, IEnumerable<T> values)

    {

        var count = 0;

        foreach (var value in values)

        {

            if (collection.AddUnique(value))

                count++;

        }

        return count;

    }



    ///<summary>

    ///	Remove an item from the collection with predicate

    ///</summary>

    ///<param name = "collection"></param>

    ///<param name = "predicate"></param>

    ///<typeparam name = "T"></typeparam>

    ///<exception cref = "ArgumentNullException"></exception>

    /// <remarks>

    /// 	Contributed by Michael T, http://about.me/MichaelTran

    /// 	Renamed by James Curran, to match corresponding HashSet.RemoveWhere()

    /// </remarks>

    public static void RemoveWhere<T>(this ICollection<T> collection, Predicate<T> predicate)

    {

        if (collection == null)

            throw new ArgumentNullException("collection");

        var deleteList = collection.Where(child => predicate(child)).ToList();

        deleteList.ForEach(t => collection.Remove(t));

    }



    /// <summary>

    /// Tests if the collection is empty.

    /// </summary>

    /// <param name="collection">The collection to test.</param>

    /// <returns>True if the collection is empty.</returns>

    public static bool IsEmpty(this ICollection collection)

    {

        return collection.Count == 0;

    }



    /// <summary>

    /// Tests if the collection is empty.

    /// </summary>

    /// <typeparam name="T">The type of the items in 

    /// the collection.</typeparam>

    /// <param name="collection">The collection to test.</param>

    /// <returns>True if the collection is empty.</returns>

    public static bool IsEmpty<T>(this ICollection<T> collection)

    {

        return collection.Count == 0;

    }



    /// <summary>

    /// Tests if the collection is empty.

    /// </summary>

    /// <param name="collection">The collection to test.</param>

    /// <returns>True if the collection is empty.</returns>

    public static bool IsEmpty(this IList collection)

    {

        return collection.Count == 0;

    }



    /// <summary>

    /// Tests if the collection is empty.

    /// </summary>

    /// <typeparam name="T">The type of the items in 

    /// the collection.</typeparam>

    /// <param name="collection">The collection to test.</param>

    /// <returns>True if the collection is empty.</returns>

    public static bool IsEmpty<T>(this IList<T> collection)

    {

        return collection.Count == 0;

    }



    /// <summary>

    /// 	Determines whether the specified value is between the the defined minimum and maximum range (including those values).

    /// </summary>

    /// <typeparam name = "T"></typeparam>

    /// <param name = "value">The value.</param>

    /// <param name = "minValue">The minimum value.</param>

    /// <param name = "maxValue">The maximum value.</param>

    /// <returns>

    /// 	<c>true</c> if the specified value is between min and max; otherwise, <c>false</c>.

    /// </returns>

    /// <example>

    /// 	var value = 5;

    /// 	if(value.IsBetween(1, 10)) { 

    /// 	// ... 

    /// 	}

    /// </example>

    public static bool IsBetween<T>(this T value, T minValue, T maxValue) where T : IComparable<T>

    {

        return IsBetween(value, minValue, maxValue, Comparer<T>.Default);

    }



    /// <summary>

    /// 	Determines whether the specified value is between the the defined minimum and maximum range (including those values).

    /// </summary>

    /// <typeparam name = "T"></typeparam>

    /// <param name = "value">The value.</param>

    /// <param name = "minValue">The minimum value.</param>

    /// <param name = "maxValue">The maximum value.</param>

    /// <param name = "comparer">An optional comparer to be used instead of the types default comparer.</param>

    /// <returns>

    /// 	<c>true</c> if the specified value is between min and max; otherwise, <c>false</c>.

    /// </returns>

    /// <example>

    /// 	var value = 5;

    /// 	if(value.IsBetween(1, 10)) {

    /// 	// ...

    /// 	}

    /// </example>

    /// <remarks>

    /// Note that this does an "inclusive" comparison:  The high & low values themselves are considered "in between".  

    /// However, in some context, a exclusive comparion -- only values greater than the low end and lesser than the high end 

    /// are "in between" -- is desired; in other contexts, values can be greater or equal to the low end, but only less than the high end.

    /// </remarks>

    public static bool IsBetween<T>(this T value, T minValue, T maxValue, IComparer<T> comparer) where T : IComparable<T>

    {

        if (comparer == null)

            throw new ArgumentNullException("comparer");



        var minMaxCompare = comparer.Compare(minValue, maxValue);

        if (minMaxCompare < 0)

            return ((comparer.Compare(value, minValue) >= 0) && (comparer.Compare(value, maxValue) <= 0));

        else

            return ((comparer.Compare(value, maxValue) >= 0) && (comparer.Compare(value, minValue) <= 0));

    }



    // todo: xml documentation is required

    public class DescendingComparer<T> : IComparer<T> where T : IComparable<T>

    {

        public int Compare(T x, T y)

        {

            return y.CompareTo(x);

        }

    }



    public class AscendingComparer<T> : IComparer<T> where T : IComparable<T>

    {

        public int Compare(T x, T y)

        {

            return x.CompareTo(y);

        }

    }



    /// <summary>

    /// Sorts the specified dictionary.

    /// </summary>

    /// <typeparam name="TKey">The type of the key.</typeparam>

    /// <typeparam name="TValue">The type of the value.</typeparam>

    /// <param name="dictionary">The dictionary.</param>

    /// <returns></returns>

    public static IDictionary<TKey, TValue> Sort<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)

    {

        if (dictionary == null)

            throw new ArgumentNullException("dictionary");

        return new SortedDictionary<TKey, TValue>(dictionary);

    }



    /// <summary>

    /// Sorts the specified dictionary.

    /// </summary>

    /// <typeparam name="TKey">The type of the key.</typeparam>

    /// <typeparam name="TValue">The type of the value.</typeparam>

    /// <param name="dictionary">The dictionary to be sorted.</param>

    /// <param name="comparer">The comparer used to sort dictionary.</param>

    /// <returns></returns>

    public static IDictionary<TKey, TValue> Sort<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)

    {

        if (dictionary == null)

            throw new ArgumentNullException("dictionary");

        if (comparer == null)

            throw new ArgumentNullException("comparer");

        return new SortedDictionary<TKey, TValue>(dictionary, comparer);

    }



    /// <summary>

    /// Sorts the dictionary by value.

    /// </summary>

    /// <typeparam name="TKey">The type of the key.</typeparam>

    /// <typeparam name="TValue">The type of the value.</typeparam>

    /// <param name="dictionary">The dictionary.</param>

    /// <returns></returns>

    public static IDictionary<TKey, TValue> SortByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)

    {

        return (new SortedDictionary<TKey, TValue>(dictionary)).OrderBy(kvp => kvp.Value).ToDictionary(item => item.Key, item => item.Value);

    }



    /// <summary>

    /// Inverts the specified dictionary. (Creates a new dictionary with the values as key, and key as values)

    /// </summary>

    /// <typeparam name="TKey">The type of the key.</typeparam>

    /// <typeparam name="TValue">The type of the value.</typeparam>

    /// <param name="dictionary">The dictionary.</param>

    /// <returns></returns>

    public static IDictionary<TValue, TKey> Invert<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)

    {

        if (dictionary == null)

            throw new ArgumentNullException("dictionary");

        return dictionary.ToDictionary(pair => pair.Value, pair => pair.Key);

    }



    /// <summary>

    /// Creates a (non-generic) Hashtable from the Dictionary.

    /// </summary>

    /// <typeparam name="TKey">The type of the key.</typeparam>

    /// <typeparam name="TValue">The type of the value.</typeparam>

    /// <param name="dictionary">The dictionary.</param>

    /// <returns></returns>

    public static Hashtable ToHashTable<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)

    {

        var table = new Hashtable();

        foreach (var item in dictionary)

            table.Add(item.Key, item.Value);

        return table;

    }



    /// <summary>

    /// Returns the value of the first entry found with one of the <paramref name="keys"/> received.

    /// <para>Returns <paramref name="defaultValue"/> if none of the keys exists in this collection </para>

    /// </summary>

    /// <param name="defaultValue">Default value if none of the keys </param>

    /// <param name="keys"> keys to search for (in order) </param>

    public static TValue GetFirstValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue defaultValue, params TKey[] keys)

    {

        foreach (var key in keys)

        {

            if (dictionary.ContainsKey(key))

                return dictionary[key];

        }

        return defaultValue;

    }



    /// <summary>

    /// Returns the value associated with the specified key, or a default value if no element is found.

    /// </summary>

    /// <typeparam name="TKey">The key data type</typeparam>

    /// <typeparam name="TValue">The value data type</typeparam>

    /// <param name="source">The source dictionary.</param>

    /// <param name="key">The key of interest.</param>

    /// <returns>The value associated with the specified key if the key is found, the default value for the value data type if the key is not found</returns>

    public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key)

    {

        return source.GetOrDefault(key, default(TValue));

    }



    /// <summary>

    /// Returns the value associated with the specified key, or the specified default value if no element is found.

    /// </summary>

    /// <typeparam name="TKey">The key data type</typeparam>

    /// <typeparam name="TValue">The value data type</typeparam>

    /// <param name="source">The source dictionary.</param>

    /// <param name="key">The key of interest.</param>

    /// <param name="defaultValue">The default value to return if the key is not found.</param>

    /// <returns>The value associated with the specified key if the key is found, the specified default value if the key is not found</returns>

    public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue defaultValue)

    {

        TValue value;

        return source.TryGetValue(key, out value) ? value : defaultValue;

    }



    /// <summary>

    /// Returns the value associated with the specified key, or throw the specified exception if no element is found.

    /// </summary>

    /// <typeparam name="TKey">The key data type</typeparam>

    /// <typeparam name="TValue">The value data type</typeparam>

    /// <param name="source">The source dictionary.</param>

    /// <param name="key">The key of interest.</param>

    /// <param name="exception">The exception to throw if the key is not found.</param>

    /// <returns>The value associated with the specified key if the key is found, the specified exception is thrown if the key is not found</returns>

    public static TValue GetOrThrow<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, Exception exception)

    {

        TValue value;

        if (source.TryGetValue(key, out value))

        {

            return value;

        }

        throw exception;

    }



    /// <summary>

    /// Tests if the collection is empty.

    /// </summary>

    /// <param name="collection">The collection to test.</param>

    /// <returns>True if the collection is empty.</returns>

    public static bool IsEmpty(this IDictionary collection)

    {

        return collection.Count == 0;

    }



    /// <summary>

    /// Tests if the IDictionary is empty.

    /// </summary>

    /// <typeparam name="TKey">The type of the key of 

    /// the IDictionary.</typeparam>

    /// <typeparam name="TValue">The type of the values

    /// of the IDictionary.</typeparam>

    /// <param name="collection">The collection to test.</param>

    /// <returns>True if the collection is empty.</returns>

    public static bool IsEmpty<TKey, TValue>(this IDictionary<TKey, TValue> collection)

    {

        return collection.Count == 0;

    }



    /// <summary>

    /// 	Gets all files in the directory matching one of the several (!) supplied patterns (instead of just one in the regular implementation).

    /// </summary>

    /// <param name = "directory">The directory.</param>

    /// <param name = "patterns">The patterns.</param>

    /// <returns>The matching files</returns>

    /// <remarks>

    /// 	This methods is quite perfect to be used in conjunction with the newly created FileInfo-Array extension methods.

    /// </remarks>

    /// <example>

    /// 	<code>

    /// 		var files = directory.GetFiles("*.txt", "*.xml");

    /// 	</code>

    /// </example>

    public static FileInfo[] GetFiles(this DirectoryInfo directory, params string[] patterns)

    {

        var files = new List<FileInfo>();

        foreach (var pattern in patterns)

            files.AddRange(directory.GetFiles(pattern));

        return files.ToArray();

    }



    /// <summary>

    /// 	Searches the provided directory recursively and returns the first file matching the provided pattern.

    /// </summary>

    /// <param name = "directory">The directory.</param>

    /// <param name = "pattern">The pattern.</param>

    /// <returns>The found file</returns>

    /// <example>

    /// 	<code>

    /// 		var directory = new DirectoryInfo(@"c:\");

    /// 		var file = directory.FindFileRecursive("win.ini");

    /// 	</code>

    /// </example>

    public static FileInfo FindFileRecursive(this DirectoryInfo directory, string pattern)

    {

        var files = directory.GetFiles(pattern);

        if (files.Length > 0)

            return files[0];



        foreach (var subDirectory in directory.GetDirectories())

        {

            var foundFile = subDirectory.FindFileRecursive(pattern);

            if (foundFile != null)

                return foundFile;

        }

        return null;

    }



    /// <summary>

    /// 	Searches the provided directory recursively and returns the first file matching to the provided predicate.

    /// </summary>

    /// <param name = "directory">The directory.</param>

    /// <param name = "predicate">The predicate.</param>

    /// <returns>The found file</returns>

    /// <example>

    /// 	<code>

    /// 		var directory = new DirectoryInfo(@"c:\");

    /// 		var file = directory.FindFileRecursive(f => f.Extension == ".ini");

    /// 	</code>

    /// </example>

    public static FileInfo FindFileRecursive(this DirectoryInfo directory, Func<FileInfo, bool> predicate)

    {

        foreach (var file in directory.GetFiles())

        {

            if (predicate(file))

                return file;

        }



        foreach (var subDirectory in directory.GetDirectories())

        {

            var foundFile = subDirectory.FindFileRecursive(predicate);

            if (foundFile != null)

                return foundFile;

        }

        return null;

    }



    /// <summary>

    /// Copies the entire directory to another one

    /// </summary>

    /// <param name="sourceDirectory">The source directory.</param>

    /// <param name="targetDirectoryPath">The target directory path.</param>

    /// <returns></returns>

    public static DirectoryInfo CopyTo(this DirectoryInfo sourceDirectory, string targetDirectoryPath)

    {

        var targetDirectory = new DirectoryInfo(targetDirectoryPath);

        CopyTo(sourceDirectory, targetDirectory);

        return targetDirectory;

    }



    /// <summary>

    /// Copies the entire directory to another one

    /// </summary>

    /// <param name="sourceDirectory">The source directory.</param>

    /// <param name="targetDirectory">The target directory.</param>

    public static void CopyTo(this DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory)

    {

        if (targetDirectory.Exists == false) targetDirectory.Create();



        foreach (var childDirectory in sourceDirectory.GetDirectories())

        {

            CopyTo(childDirectory, Path.Combine(targetDirectory.FullName, childDirectory.Name));

        }



        foreach (var file in sourceDirectory.GetFiles())

        {

            file.CopyTo(Path.Combine(targetDirectory.FullName, file.Name));

        }

    }



    /// <summary>Checks whether the value is in range</summary>

    /// <param name="value">The Value</param>

    /// <param name="minValue">The minimum value</param>

    /// <param name="maxValue">The maximum value</param>

    public static bool InRange(this double value, double minValue, double maxValue)

    {

        return (value >= minValue && value <= maxValue);

    }



    /// <summary>Checks whether the value is in range or returns the default value</summary>

    /// <param name="value">The Value</param>

    /// <param name="minValue">The minimum value</param>

    /// <param name="maxValue">The maximum value</param>

    /// <param name="defaultValue">The default value</param>

    public static double InRange(this double value, double minValue, double maxValue, double defaultValue)

    {

        return value.InRange(minValue, maxValue) ? value : defaultValue;

    }



    /// <summary>

    /// Gets a TimeSpan from a double number of days.

    /// </summary>

    /// <param name="days">The number of days the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of days.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Days(this double days)

    {

        return TimeSpan.FromDays(days);

    }



    /// <summary>

    /// Gets a TimeSpan from a double number of hours.

    /// </summary>

    /// <param name="days">The number of hours the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of hours.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Hours(this double hours)

    {

        return TimeSpan.FromHours(hours);

    }



    /// <summary>

    /// Gets a TimeSpan from a double number of milliseconds.

    /// </summary>

    /// <param name="days">The number of milliseconds the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of milliseconds.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Milliseconds(this double milliseconds)

    {

        return TimeSpan.FromMilliseconds(milliseconds);

    }



    /// <summary>

    /// Gets a TimeSpan from a double number of minutes.

    /// </summary>

    /// <param name="days">The number of minutes the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of minutes.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Minutes(this double minutes)

    {

        return TimeSpan.FromMinutes(minutes);

    }



    /// <summary>

    /// Gets a TimeSpan from a double number of seconds.

    /// </summary>

    /// <param name="days">The number of seconds the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of seconds.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Seconds(this double seconds)

    {

        return TimeSpan.FromSeconds(seconds);

    }



    /// <summary>

    /// 	Renames a file.

    /// </summary>

    /// <param name = "file">The file.</param>

    /// <param name = "newName">The new name.</param>

    /// <returns>The renamed file</returns>

    /// <example>

    /// 	<code>

    /// 		var file = new FileInfo(@"c:\test.txt");

    /// 		file.Rename("test2.txt");

    /// 	</code>

    /// </example>

    public static FileInfo Rename(this FileInfo file, string newName)

    {

        var filePath = Path.Combine(Path.GetDirectoryName(file.FullName), newName);

        file.MoveTo(filePath);

        return file;

    }



    /// <summary>

    /// 	Renames a without changing its extension.

    /// </summary>

    /// <param name = "file">The file.</param>

    /// <param name = "newName">The new name.</param>

    /// <returns>The renamed file</returns>

    /// <example>

    /// 	<code>

    /// 		var file = new FileInfo(@"c:\test.txt");

    /// 		file.RenameFileWithoutExtension("test3");

    /// 	</code>

    /// </example>

    public static FileInfo RenameFileWithoutExtension(this FileInfo file, string newName)

    {

        var fileName = string.Concat(newName, file.Extension);

        file.Rename(fileName);

        return file;

    }



    /// <summary>

    /// 	Deletes several files at once and consolidates any exceptions.

    /// </summary>

    /// <param name = "files">The files.</param>

    /// <example>

    /// 	<code>

    /// 		var files = directory.GetFiles("*.txt", "*.xml");

    /// 		files.Delete()

    /// 	</code>

    /// </example>

    public static void Delete(this FileInfo[] files)

    {

        files.Delete(true);

    }



    /// <summary>

    /// 	Deletes several files at once and optionally consolidates any exceptions.

    /// </summary>

    /// <param name = "files">The files.</param>

    /// <param name = "consolidateExceptions">if set to <c>true</c> exceptions are consolidated and the processing is not interrupted.</param>

    /// <example>

    /// 	<code>

    /// 		var files = directory.GetFiles("*.txt", "*.xml");

    /// 		files.Delete()

    /// 	</code>

    /// </example>

    public static void Delete(this FileInfo[] files, bool consolidateExceptions)

    {



        if (consolidateExceptions)

        {

            List<Exception> exceptions = new List<Exception>();



            foreach (var file in files)

            {

                try

                {

                    file.Delete();

                }

                catch (Exception e)

                {

                    exceptions.Add(e);

                }

            }



            if (exceptions.Any())

                throw exceptions[0];

        }

        else

        {

            foreach (var file in files)

            {

                file.Delete();

            }

        }

    }





    /// <summary>

    /// 	Copies several files to a new folder at once and consolidates any exceptions.

    /// </summary>

    /// <param name = "files">The files.</param>

    /// <param name = "targetPath">The target path.</param>

    /// <returns>The newly created file copies</returns>

    /// <example>

    /// 	<code>

    /// 		var files = directory.GetFiles("*.txt", "*.xml");

    /// 		var copiedFiles = files.CopyTo(@"c:\temp\");

    /// 	</code>

    /// </example>

    public static FileInfo[] CopyTo(this FileInfo[] files, string targetPath)

    {

        return files.CopyTo(targetPath, true);

    }



    /// <summary>

    /// 	Copies several files to a new folder at once and optionally consolidates any exceptions.

    /// </summary>

    /// <param name = "files">The files.</param>

    /// <param name = "targetPath">The target path.</param>

    /// <param name = "consolidateExceptions">if set to <c>true</c> exceptions are consolidated and the processing is not interrupted.</param>

    /// <returns>The newly created file copies</returns>

    /// <example>

    /// 	<code>

    /// 		var files = directory.GetFiles("*.txt", "*.xml");

    /// 		var copiedFiles = files.CopyTo(@"c:\temp\");

    /// 	</code>

    /// </example>

    public static FileInfo[] CopyTo(this FileInfo[] files, string targetPath, bool consolidateExceptions)

    {

        var copiedfiles = new List<FileInfo>();

        List<Exception> exceptions = null;



        foreach (var file in files)

        {

            try

            {

                var fileName = Path.Combine(targetPath, file.Name);

                copiedfiles.Add(file.CopyTo(fileName));

            }

            catch (Exception e)

            {

                if (consolidateExceptions)

                {

                    if (exceptions == null)

                        exceptions = new List<Exception>();

                    exceptions.Add(e);

                }

                else

                    throw;

            }

        }



        if ((exceptions != null) && (exceptions.Count > 0))

            throw exceptions[0];



        return copiedfiles.ToArray();

    }



    /// <summary>

    /// 	Moves several files to a new folder at once and optionally consolidates any exceptions.

    /// </summary>

    /// <param name = "files">The files.</param>

    /// <param name = "targetPath">The target path.</param>

    /// <returns>The moved files</returns>

    /// <example>

    /// 	<code>

    /// 		var files = directory.GetFiles("*.txt", "*.xml");

    /// 		files.MoveTo(@"c:\temp\");

    /// 	</code>

    /// </example>

    public static FileInfo[] MoveTo(this FileInfo[] files, string targetPath)

    {

        return files.MoveTo(targetPath, true);

    }



    /// <summary>

    /// 	Movies several files to a new folder at once and optionally consolidates any exceptions.

    /// </summary>

    /// <param name = "files">The files.</param>

    /// <param name = "targetPath">The target path.</param>

    /// <param name = "consolidateExceptions">if set to <c>true</c> exceptions are consolidated and the processing is not interrupted.</param>

    /// <returns>The moved files</returns>

    /// <example>

    /// 	<code>

    /// 		var files = directory.GetFiles("*.txt", "*.xml");

    /// 		files.MoveTo(@"c:\temp\");

    /// 	</code>

    /// </example>

    public static FileInfo[] MoveTo(this FileInfo[] files, string targetPath, bool consolidateExceptions)

    {

        List<Exception> exceptions = null;



        foreach (var file in files)

        {

            try

            {

                var fileName = Path.Combine(targetPath, file.Name);

                file.MoveTo(fileName);

            }

            catch (Exception e)

            {

                if (consolidateExceptions)

                {

                    if (exceptions == null)

                        exceptions = new List<Exception>();

                    exceptions.Add(e);

                }

                else

                    throw;

            }

        }



        if ((exceptions != null) && (exceptions.Count > 0))

            throw exceptions[0];



        return files;

    }



    /// <summary>

    /// 	Sets file attributes for several files at once

    /// </summary>

    /// <param name = "files">The files.</param>

    /// <param name = "attributes">The attributes to be set.</param>

    /// <returns>The changed files</returns>

    /// <example>

    /// 	<code>

    /// 		var files = directory.GetFiles("*.txt", "*.xml");

    /// 		files.SetAttributes(FileAttributes.Archive);

    /// 	</code>

    /// </example>

    public static FileInfo[] SetAttributes(this FileInfo[] files, FileAttributes attributes)

    {

        foreach (var file in files)

            file.Attributes = attributes;

        return files;

    }



    /// <summary>

    /// 	Appends file attributes for several files at once (additive to any existing attributes)

    /// </summary>

    /// <param name = "files">The files.</param>

    /// <param name = "attributes">The attributes to be set.</param>

    /// <returns>The changed files</returns>

    /// <example>

    /// 	<code>

    /// 		var files = directory.GetFiles("*.txt", "*.xml");

    /// 		files.SetAttributesAdditive(FileAttributes.Archive);

    /// 	</code>

    /// </example>

    public static FileInfo[] SetAttributesAdditive(this FileInfo[] files, FileAttributes attributes)

    {

        foreach (var file in files)

            file.Attributes = (file.Attributes | attributes);

        return files;

    }



    /// <summary>Checks whether the value is in range</summary>

    /// <param name="value">The Value</param>

    /// <param name="minValue">The minimum value</param>

    /// <param name="maxValue">The maximum value</param>

    public static bool InRange(this float value, float minValue, float maxValue)

    {

        return (value >= minValue && value <= maxValue);

    }



    /// <summary>Checks whether the value is in range or returns the default value</summary>

    /// <param name="value">The Value</param>

    /// <param name="minValue">The minimum value</param>

    /// <param name="maxValue">The maximum value</param>

    /// <param name="defaultValue">The default value</param>

    public static float InRange(this float value, float minValue, float maxValue, float defaultValue)

    {

        return value.InRange(minValue, maxValue) ? value : defaultValue;

    }



    /// <summary>

    /// Gets a TimeSpan from a float number of days.

    /// </summary>

    /// <param name="days">The number of days the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of days.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Days(this float days)

    {

        return TimeSpan.FromDays(days);

    }



    /// <summary>

    /// Gets a TimeSpan from a float number of hours.

    /// </summary>

    /// <param name="days">The number of hours the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of hours.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Hours(this float hours)

    {

        return TimeSpan.FromHours(hours);

    }



    /// <summary>

    /// Gets a TimeSpan from a float number of milliseconds.

    /// </summary>

    /// <param name="days">The number of milliseconds the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of milliseconds.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Milliseconds(this float milliseconds)

    {

        return TimeSpan.FromMilliseconds(milliseconds);

    }



    /// <summary>

    /// Gets a TimeSpan from a float number of minutes.

    /// </summary>

    /// <param name="days">The number of minutes the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of minutes.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Minutes(this float minutes)

    {

        return TimeSpan.FromMinutes(minutes);

    }



    /// <summary>

    /// Gets a TimeSpan from a float number of seconds.

    /// </summary>

    /// <param name="days">The number of seconds the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of seconds.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Seconds(this float seconds)

    {

        return TimeSpan.FromSeconds(seconds);

    }



    /// <summary>

    /// 	Performs the specified action n times based on the underlying int value.

    /// </summary>

    /// <param name = "value">The value.</param>

    /// <param name = "action">The action.</param>

    public static void Times(this int value, Action action)

    {

        for (var i = 0; i < value; i++)

            action();

    }



    /// <summary>

    /// 	Performs the specified action n times based on the underlying int value.

    /// </summary>

    /// <param name = "value">The value.</param>

    /// <param name = "action">The action.</param>

    public static void Times(this int value, Action<int> action)

    {

        for (var i = 0; i < value; i++)

            action(i);

    }



    /// <summary>

    /// 	Determines whether the value is even

    /// </summary>

    /// <param name = "value">The Value</param>

    /// <returns>true or false</returns>

    public static bool IsEven(this int value)

    {

        return value.AsLong().IsEven();

    }



    /// <summary>

    /// 	Determines whether the value is odd

    /// </summary>

    /// <param name = "value">The Value</param>

    /// <returns>true or false</returns>

    public static bool IsOdd(this int value)

    {

        return value.AsLong().IsOdd();

    }



    /// <summary>Checks whether the value is in range</summary>

    /// <param name="value">The Value</param>

    /// <param name="minValue">The minimum value</param>

    /// <param name="maxValue">The maximum value</param>

    public static bool InRange(this int value, int minValue, int maxValue)

    {

        return value.AsLong().InRange(minValue, maxValue);

    }



    /// <summary>Checks whether the value is in range or returns the default value</summary>

    /// <param name="value">The Value</param>

    /// <param name="minValue">The minimum value</param>

    /// <param name="maxValue">The maximum value</param>

    /// <param name="defaultValue">The default value</param>

    public static int InRange(this int value, int minValue, int maxValue, int defaultValue)

    {

        return (int)value.AsLong().InRange(minValue, maxValue, defaultValue);

    }



    /// <summary>

    /// A prime number (or a prime) is a natural number that has exactly two distinct natural number divisors: 1 and itself.

    /// </summary>

    /// <param name="candidate">Object value</param>

    /// <returns>Returns true if the value is a prime number.</returns>

    public static bool IsPrime(this int candidate)

    {

        return candidate.AsLong().IsPrime();

    }



    /// <summary>

    /// Converts the value to ordinal string. (English)

    /// </summary>

    /// <param name="i">Object value</param>

    /// <returns>Returns string containing ordinal indicator adjacent to a numeral denoting. (English)</returns>

    public static string ToOrdinal(this int i)

    {

        return i.AsLong().ToOrdinal();

    }



    /// <summary>

    /// Converts the value to ordinal string with specified format. (English)

    /// </summary>

    /// <param name="i">Object value</param>

    /// <param name="format">A standard or custom format string that is supported by the object to be formatted.</param>

    /// <returns>Returns string containing ordinal indicator adjacent to a numeral denoting. (English)</returns>

    public static string ToOrdinal(this int i, string format)

    {

        return i.AsLong().ToOrdinal(format);

    }



    /// <summary>

    /// Returns the integer as long.

    /// </summary>

    public static long AsLong(this int i)

    {

        return i;

    }



    /// <summary>

    /// To check whether an index is in the range of the given array.

    /// </summary>

    /// <param name="index">Index to check</param>

    /// <param name="arrayToCheck">Array where to check</param>

    /// <returns></returns>

    /// <remarks>

    /// 	Contributed by Mohammad Rahman, http://mohammad-rahman.blogspot.com/

    /// </remarks>

    public static bool IsIndexInArray(this int index, Array arrayToCheck)

    {

        return index.GetArrayIndex().InRange(arrayToCheck.GetLowerBound(0), arrayToCheck.GetUpperBound(0));

    }



    /// <summary>

    /// To get Array index from a given based on a number.

    /// </summary>

    /// <param name="at">Real world postion </param>

    /// <returns></returns>

    /// <remarks>

    /// 	Contributed by Mohammad Rahman, http://mohammad-rahman.blogspot.com/

    /// 	jceddy fixed typo

    /// </remarks>

    public static int GetArrayIndex(this int at)

    {

        return at == 0 ? 0 : at - 1;

    }



    /// <summary>

    /// Gets a TimeSpan from an integer number of days.

    /// </summary>

    /// <param name="days">The number of days the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of days.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Days(this int days)

    {

        return TimeSpan.FromDays(days);

    }



    /// <summary>

    /// Gets a TimeSpan from an integer number of hours.

    /// </summary>

    /// <param name="days">The number of hours the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of hours.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Hours(this int hours)

    {

        return TimeSpan.FromHours(hours);

    }



    /// <summary>

    /// Gets a TimeSpan from an integer number of milliseconds.

    /// </summary>

    /// <param name="days">The number of milliseconds the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of milliseconds.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Milliseconds(this int milliseconds)

    {

        return TimeSpan.FromMilliseconds(milliseconds);

    }



    /// <summary>

    /// Gets a TimeSpan from an integer number of minutes.

    /// </summary>

    /// <param name="days">The number of minutes the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of minutes.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Minutes(this int minutes)

    {

        return TimeSpan.FromMinutes(minutes);

    }



    /// <summary>

    /// Gets a TimeSpan from an integer number of seconds.

    /// </summary>

    /// <param name="days">The number of seconds the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of seconds.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Seconds(this int seconds)

    {

        return TimeSpan.FromSeconds(seconds);

    }



    /// <summary>

    /// Gets a TimeSpan from an integer number of ticks.

    /// </summary>

    /// <param name="days">The number of ticks the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of ticks.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Ticks(this int ticks)

    {

        return TimeSpan.FromTicks(ticks);

    }



    /// <summary>

    /// 	Performs the specified action n times based on the underlying long value.

    /// </summary>

    /// <param name = "value">The value.</param>

    /// <param name = "action">The action.</param>

    public static void Times(this long value, Action action)

    {

        for (var i = 0; i < value; i++)

            action();

    }



    /// <summary>

    /// 	Performs the specified action n times based on the underlying long value.

    /// </summary>

    /// <param name = "value">The value.</param>

    /// <param name = "action">The action.</param>

    public static void Times(this long value, Action<long> action)

    {

        for (var i = 0; i < value; i++)

            action(i);

    }



    /// <summary>

    /// 	Determines whether the value is even

    /// </summary>

    /// <param name = "value">The Value</param>

    /// <returns>true or false</returns>

    public static bool IsEven(this long value)

    {

        return value % 2 == 0;

    }



    /// <summary>

    /// 	Determines whether the value is odd

    /// </summary>

    /// <param name = "value">The Value</param>

    /// <returns>true or false</returns>

    public static bool IsOdd(this long value)

    {

        return value % 2 != 0;

    }



    /// <summary>Checks whether the value is in range</summary>

    /// <param name="value">The Value</param>

    /// <param name="minValue">The minimum value</param>

    /// <param name="maxValue">The maximum value</param>

    public static bool InRange(this long value, long minValue, long maxValue)

    {

        return (value >= minValue && value <= maxValue);

    }



    /// <summary>Checks whether the value is in range or returns the default value</summary>

    /// <param name="value">The Value</param>

    /// <param name="minValue">The minimum value</param>

    /// <param name="maxValue">The maximum value</param>

    /// <param name="defaultValue">The default value</param>

    public static long InRange(this long value, long minValue, long maxValue, long defaultValue)

    {

        return value.InRange(minValue, maxValue) ? value : defaultValue;

    }



    /// <summary>

    /// A prime number (or a prime) is a natural number that has exactly two distinct natural number divisors: 1 and itself.

    /// </summary>

    /// <param name="candidate">Object value</param>

    /// <returns>Returns true if the value is a prime number.</returns>

    public static bool IsPrime(this long candidate)

    {

        if ((candidate & 1) == 0)

        {

            if (candidate == 2)

            {

                return true;

            }

            else

            {

                return false;

            }

        }



        for (long i = 3; (i * i) <= candidate; i += 2)

        {

            if ((candidate % i) == 0)

            {

                return false;

            }

        }



        return candidate != 1;

    }



    /// <summary>

    /// Converts the value to ordinal string. (English)

    /// </summary>

    /// <param name="i">Object value</param>

    /// <returns>Returns string containing ordinal indicator adjacent to a numeral denoting. (English)</returns>

    public static string ToOrdinal(this long i)

    {

        string suffix = "°";

        return string.Format("{0}{1}", i, suffix);

    }



    /// <summary>

    /// Converts the value to ordinal string with specified format. (English)

    /// </summary>

    /// <param name="i">Object value</param>

    /// <param name="format">A standard or custom format string that is supported by the object to be formatted.</param>

    /// <returns>Returns string containing ordinal indicator adjacent to a numeral denoting. (English)</returns>

    public static string ToOrdinal(this long i, string format)

    {

        return string.Format(format, i.ToOrdinal());

    }



    /// <summary>

    /// Gets a TimeSpan from a long number of days.

    /// </summary>

    /// <param name="days">The number of days the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of days.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Days(this long days)

    {

        return TimeSpan.FromDays(days);

    }



    /// <summary>

    /// Gets a TimeSpan from a long number of hours.

    /// </summary>

    /// <param name="days">The number of hours the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of hours.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Hours(this long hours)

    {

        return TimeSpan.FromHours(hours);

    }



    /// <summary>

    /// Gets a TimeSpan from a long number of milliseconds.

    /// </summary>

    /// <param name="days">The number of milliseconds the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of milliseconds.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Milliseconds(this long milliseconds)

    {

        return TimeSpan.FromMilliseconds(milliseconds);

    }



    /// <summary>

    /// Gets a TimeSpan from a long number of minutes.

    /// </summary>

    /// <param name="days">The number of minutes the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of minutes.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Minutes(this long minutes)

    {

        return TimeSpan.FromMinutes(minutes);

    }



    /// <summary>

    /// Gets a TimeSpan from a long number of seconds.

    /// </summary>

    /// <param name="days">The number of seconds the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of seconds.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Seconds(this long seconds)

    {

        return TimeSpan.FromSeconds(seconds);

    }



    /// <summary>

    /// Gets a TimeSpan from a long number of ticks.

    /// </summary>

    /// <param name="days">The number of ticks the TimeSpan will contain.</param>

    /// <returns>A TimeSpan containing the specified number of ticks.</returns>

    /// <remarks>

    ///		Contributed by jceddy

    /// </remarks>

    public static TimeSpan Ticks(this long ticks)

    {

        return TimeSpan.FromTicks(ticks);

    }



    /// <summary>

    /// 	Inserts an item uniquely to to a list and returns a value whether the item was inserted or not.

    /// </summary>

    /// <typeparam name = "T">The generic list item type.</typeparam>

    /// <param name = "list">The list to be inserted into.</param>

    /// <param name = "index">The index to insert the item at.</param>

    /// <param name = "item">The item to be added.</param>

    /// <returns>Indicates whether the item was inserted or not</returns>

    public static bool InsertUnique<T>(this IList<T> list, int index, T item)

    {

        if (list.Contains(item) == false)

        {

            list.Insert(index, item);

            return true;

        }

        return false;

    }



    /// <summary>

    /// 	Inserts a range of items uniquely to a list starting at a given index and returns the amount of items inserted.

    /// </summary>

    /// <typeparam name = "T">The generic list item type.</typeparam>

    /// <param name = "list">The list to be inserted into.</param>

    /// <param name = "startIndex">The start index.</param>

    /// <param name = "items">The items to be inserted.</param>

    /// <returns>The amount if items that were inserted.</returns>

    public static int InsertRangeUnique<T>(this IList<T> list, int startIndex, IEnumerable<T> items)

    {

        var index = startIndex + items.Reverse().Count(item => list.InsertUnique(startIndex, item));

        return (index - startIndex);

    }



    /// <summary>

    /// 	Return the index of the first matching item or -1.

    /// </summary>

    /// <typeparam name = "T"></typeparam>

    /// <param name = "list">The list.</param>

    /// <param name = "comparison">The comparison.</param>

    /// <returns>The item index</returns>

    public static int IndexOf<T>(this IList<T> list, Func<T, bool> comparison)

    {

        for (var i = 0; i < list.Count; i++)

        {

            if (comparison(list[i]))

                return i;

        }

        return -1;

    }



    /// <summary>

    /// 	Join all the elements in the list and create a string seperated by the specified char.

    /// </summary>

    /// <param name = "list">

    /// 	The list.

    /// </param>

    /// <param name = "joinChar">

    /// 	The join char.

    /// </param>

    /// <typeparam name = "T">

    /// </typeparam>

    /// <returns>

    /// 	The resulting string of the elements in the list.

    /// </returns>

    /// <remarks>

    /// 	Contributed by Michael T, http://about.me/MichaelTran

    /// </remarks>

    public static string Join<T>(this IList<T> list, char joinChar)

    {

        return list.Join(joinChar.ToString());

    }



    /// <summary>

    /// 	Join all the elements in the list and create a string seperated by the specified string.

    /// </summary>

    /// <param name = "list">

    /// 	The list.

    /// </param>

    /// <param name = "joinString">

    /// 	The join string.

    /// </param>

    /// <typeparam name = "T">

    /// </typeparam>

    /// <returns>

    /// 	The resulting string of the elements in the list.

    /// </returns>

    /// <remarks>

    /// 	Contributed by Michael T, http://about.me/MichaelTran

    /// 	Optimised by Mario Majcica

    /// </remarks>

    public static string Join<T>(this IList<T> list, string joinString)

    {

        if (list == null || !list.Any())

            return String.Empty;



        StringBuilder result = new StringBuilder();



        int listCount = list.Count;

        int listCountMinusOne = listCount - 1;



        if (listCount > 1)

        {

            for (var i = 0; i < listCount; i++)

            {

                if (i != listCountMinusOne)

                {

                    result.Append(list[i]);

                    result.Append(joinString);

                }

                else

                    result.Append(list[i]);

            }

        }

        else

            result.Append(list[0]);



        return result.ToString();

    }





    ///<summary>

    ///	Cast this list into a List

    ///</summary>

    ///<param name = "source"></param>

    ///<typeparam name = "T"></typeparam>

    ///<returns></returns>

    /// <remarks>

    /// 	Contributed by Michael T, http://about.me/MichaelTran

    /// </remarks>

    public static List<T> Cast<T>(this IList source)

    {

        var list = new List<T>();

        list.AddRange(source.OfType<T>());

        return list;

    }



    /// <summary>

    /// Get's an random item from list.

    /// </summary>

    /// <typeparam name="T">Type of list item.</typeparam>

    /// <param name="source">Source list.</param>

    /// <returns>A random item from list.</returns>

    public static T GetRandomItem<T>(this IList<T> source)

    {

        if (source.Count > 0)

            // The maxValue for the upper-bound in the Next() method is exclusive, see: http://stackoverflow.com/q/5063269/375958

            return source[UnityEngine.Random.Range(0, source.Count)];

        else

            throw new InvalidOperationException("Could not get item from empty list.");

    }



    /// <summary>

    /// Get's an random item from list.

    /// </summary>

    /// <typeparam name="T">Type of list item.</typeparam>

    /// <param name="source">Source list.</param>

    /// <param name="seed">MSDN: A number used to calculate a starting value for the pseudo-random number 

    /// sequence. If a negative number is specified, the absolute value of the number is used..</param>

    /// <returns>A random item from list.</returns>

    public static T GetRandomItem<T>(this IList<T> source, int seed)

    {

        UnityEngine.Random.InitState(seed);

        return source.GetRandomItem();

    }



    #region Merge



    /// <summary>The merge.</summary>

    /// <param name="lists">The lists.</param>

    /// <typeparam name="T"></typeparam>

    /// <returns></returns>

    /// <remarks>

    /// 	Contributed by Michael T, http://about.me/MichaelTran

    /// </remarks>

    public static List<T> Merge<T>(params List<T>[] lists)

    {

        var merged = new List<T>();

        foreach (var list in lists) merged.Merge(list);

        return merged;

    }



    /// <summary>The merge.</summary>

    /// <param name="match">The match.</param>

    /// <param name="lists">The lists.</param>

    /// <typeparam name="T"></typeparam>

    /// <returns></returns>

    /// <remarks>

    /// 	Contributed by Michael T, http://about.me/MichaelTran

    /// </remarks>

    public static List<T> Merge<T>(Expression<Func<T, object>> match, params List<T>[] lists)

    {

        var merged = new List<T>();

        foreach (var list in lists) merged.Merge(list, match);

        return merged;

    }



    /// <summary>The merge.</summary>

    /// <param name="list1">The list 1.</param>

    /// <param name="list2">The list 2.</param>

    /// <param name="match">The match.</param>

    /// <typeparam name="T"></typeparam>

    /// <returns></returns>

    /// <remarks>

    /// 	Contributed by Michael T, http://about.me/MichaelTran

    /// </remarks>

    public static List<T> Merge<T>(this List<T> list1, List<T> list2, Expression<Func<T, object>> match)

    {

        if (list1 != null && list2 != null && match != null)

        {

            var matchFunc = match.Compile();

            foreach (var item in list2)

            {

                var key = matchFunc(item);

                if (!list1.Exists(i => matchFunc(i).Equals(key))) list1.Add(item);

            }

        }



        return list1;

    }



    /// <summary>The merge.</summary>

    /// <param name="list1">The list 1.</param>

    /// <param name="list2">The list 2.</param>

    /// <typeparam name="T"></typeparam>

    /// <returns></returns>

    /// <remarks>

    /// 	Contributed by Michael T, http://about.me/MichaelTran

    /// </remarks>

    public static List<T> Merge<T>(this List<T> list1, List<T> list2)

    {

        if (list1 != null && list2 != null) foreach (var item in list2.Where(item => !list1.Contains(item))) list1.Add(item);

        return list1;

    }



    #endregion



    /// <summary>

    /// 	Returns a combined value of strings from a string array

    /// </summary>

    /// <param name = "values">The values.</param>

    /// <param name = "prefix">The prefix.</param>

    /// <param name = "suffix">The suffix.</param>

    /// <param name = "quotation">The quotation (or null).</param>

    /// <param name = "separator">The separator.</param>

    /// <returns>

    /// 	A <see cref = "System.String" /> that represents this instance.

    /// </returns>

    /// <remarks>

    /// 	Contributed by blaumeister, http://www.codeplex.com/site/users/view/blaumeiser

    /// </remarks>

    public static string ToString(this string[] values, string prefix = "(", string suffix = ")", string quotation = "\"", string separator = ",")

    {

        var sb = new StringBuilder();

        sb.Append(prefix);



        for (var i = 0; i < values.Length; i++)

        {

            if (i > 0)

                sb.Append(separator);

            if (quotation != null)

                sb.Append(quotation);

            sb.Append(values[i]);

            if (quotation != null)

                sb.Append(quotation);

        }



        sb.Append(suffix);

        return sb.ToString();

    }



    /// <summary>

    /// 	The method provides an iterator through all lines of the text reader.

    /// </summary>

    /// <param name = "reader">The text reader.</param>

    /// <returns>The iterator</returns>

    /// <example>

    /// 	<code>

    /// 		using(var reader = fileInfo.OpenText()) {

    /// 		foreach(var line in reader.IterateLines()) {

    /// 		// ...

    /// 		}

    /// 		}

    /// 	</code>

    /// </example>

    /// <remarks>

    /// 	Contributed by OlivierJ

    /// </remarks>

    public static IEnumerable<string> IterateLines(this TextReader reader)

    {

        string line = null;

        while ((line = reader.ReadLine()) != null)

            yield return line;

    }



    /// <summary>

    /// 	The method executes the passed delegate /lambda expression) for all lines of the text reader.

    /// </summary>

    /// <param name = "reader">The text reader.</param>

    /// <param name = "action">The action.</param>

    /// <example>

    /// 	<code>

    /// 		using(var reader = fileInfo.OpenText()) {

    /// 		reader.IterateLines(l => Console.WriteLine(l));

    /// 		}

    /// 	</code>

    /// </example>

    /// <remarks>

    /// 	Contributed by OlivierJ

    /// </remarks>

    public static void IterateLines(this TextReader reader, Action<string> action)

    {

        foreach (var line in reader.IterateLines())

            action(line);

    }



    #region Common string extensions



    /// <summary>

    /// 	Determines whether the specified string is null or empty.

    /// </summary>

    /// <param name = "value">The string value to check.</param>

    public static bool IsEmpty(this string value)

    {

        return ((value == null) || (value.Length == 0));

    }



    /// <summary>

    /// 	Determines whether the specified string is not null or empty.

    /// </summary>

    /// <param name = "value">The string value to check.</param>

    public static bool IsNotEmpty(this string value)

    {

        return (value.IsEmpty() == false);

    }



    /// <summary>

    /// 	Checks whether the string is empty and returns a default value in case.

    /// </summary>

    /// <param name = "value">The string to check.</param>

    /// <param name = "defaultValue">The default value.</param>

    /// <returns>Either the string or the default value.</returns>

    public static string IfEmpty(this string value, string defaultValue)

    {

        return (value.IsNotEmpty() ? value : defaultValue);

    }



    /// <summary>

    /// 	Formats the value with the parameters using string.Format.

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <param name = "parameters">The parameters.</param>

    /// <returns></returns>

    public static string FormatWith(this string value, params object[] parameters)

    {

        return string.Format(value, parameters);

    }



    /// <summary>

    /// 	Trims the text to a provided maximum length.

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <param name = "maxLength">Maximum length.</param>

    /// <returns></returns>

    /// <remarks>

    /// 	Proposed by Rene Schulte

    /// </remarks>

    public static string TrimToMaxLength(this string value, int maxLength)

    {

        return (value == null || value.Length <= maxLength ? value : value.Substring(0, maxLength));

    }



    /// <summary>

    /// 	Trims the text to a provided maximum length and adds a suffix if required.

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <param name = "maxLength">Maximum length.</param>

    /// <param name = "suffix">The suffix.</param>

    /// <returns></returns>

    /// <remarks>

    /// 	Proposed by Rene Schulte

    /// </remarks>

    public static string TrimToMaxLength(this string value, int maxLength, string suffix)

    {

        return (value == null || value.Length <= maxLength ? value : string.Concat(value.Substring(0, maxLength), suffix));

    }



    /// <summary>

    /// 	Determines whether the comparison value strig is contained within the input value string

    /// </summary>

    /// <param name = "inputValue">The input value.</param>

    /// <param name = "comparisonValue">The comparison value.</param>

    /// <param name = "comparisonType">Type of the comparison to allow case sensitive or insensitive comparison.</param>

    /// <returns>

    /// 	<c>true</c> if input value contains the specified value, otherwise, <c>false</c>.

    /// </returns>

    public static bool Contains(this string inputValue, string comparisonValue, StringComparison comparisonType)

    {

        return (inputValue.IndexOf(comparisonValue, comparisonType) != -1);

    }



    /// <summary>

    /// 	Determines whether the comparison value string is contained within the input value string without any

    ///     consideration about the case (<see cref="StringComparison.InvariantCultureIgnoreCase"/>).

    /// </summary>

    /// <param name = "inputValue">The input value.</param>

    /// <param name = "comparisonValue">The comparison value.  Case insensitive</param>

    /// <returns>

    /// 	<c>true</c> if input value contains the specified value (case insensitive), otherwise, <c>false</c>.

    /// </returns>

    public static bool ContainsEquivalenceTo(this string inputValue, string comparisonValue)

    {

        return BothStringsAreEmpty(inputValue, comparisonValue) || StringContainsEquivalence(inputValue, comparisonValue);

    }



    /// <summary>

    /// Centers a charters in this string, padding in both, left and right, by specified Unicode character,

    /// for a specified total lenght.

    /// </summary>

    /// <param name="value">Instance value.</param>

    /// <param name="width">The number of characters in the resulting string, 

    /// equal to the number of original characters plus any additional padding characters.

    /// </param>

    /// <param name="padChar">A Unicode padding character.</param>

    /// <param name="truncate">Should get only the substring of specified width if string width is 

    /// more than the specified width.</param>

    /// <returns>A new string that is equivalent to this instance, 

    /// but center-aligned with as many paddingChar characters as needed to create a 

    /// length of width paramether.</returns>

    public static string PadBoth(this string value, int width, char padChar, bool truncate = false)

    {

        int diff = width - value.Length;

        if (diff == 0 || diff < 0 && !(truncate))

        {

            return value;

        }

        else if (diff < 0)

        {

            return value.Substring(0, width);

        }

        else

        {

            return value.PadLeft(width - diff / 2, padChar).PadRight(width, padChar);

        }

    }



    /// <summary>

    /// 	Reverses / mirrors a string.

    /// </summary>

    /// <param name = "value">The string to be reversed.</param>

    /// <returns>The reversed string</returns>

    public static string Reverse(this string value)

    {

        if (value.IsEmpty() || (value.Length == 1))

            return value;



        var chars = value.ToCharArray();

        Array.Reverse(chars);

        return new string(chars);

    }



    /// <summary>

    /// 	Ensures that a string starts with a given prefix.

    /// </summary>

    /// <param name = "value">The string value to check.</param>

    /// <param name = "prefix">The prefix value to check for.</param>

    /// <returns>The string value including the prefix</returns>

    /// <example>

    /// 	<code>

    /// 		var extension = "txt";

    /// 		var fileName = string.Concat(file.Name, extension.EnsureStartsWith("."));

    /// 	</code>

    /// </example>

    public static string EnsureStartsWith(this string value, string prefix)

    {

        return value.StartsWith(prefix) ? value : string.Concat(prefix, value);

    }



    /// <summary>

    /// 	Ensures that a string ends with a given suffix.

    /// </summary>

    /// <param name = "value">The string value to check.</param>

    /// <param name = "suffix">The suffix value to check for.</param>

    /// <returns>The string value including the suffix</returns>

    /// <example>

    /// 	<code>

    /// 		var url = "http://www.pgk.de";

    /// 		url = url.EnsureEndsWith("/"));

    /// 	</code>

    /// </example>

    public static string EnsureEndsWith(this string value, string suffix)

    {

        return value.EndsWith(suffix) ? value : string.Concat(value, suffix);

    }



    /// <summary>

    /// 	Repeats the specified string value as provided by the repeat count.

    /// </summary>

    /// <param name = "value">The original string.</param>

    /// <param name = "repeatCount">The repeat count.</param>

    /// <returns>The repeated string</returns>

    public static string Repeat(this string value, int repeatCount)

    {

        if (value.Length == 1)

            return new string(value[0], repeatCount);



        var sb = new StringBuilder(repeatCount * value.Length);

        while (repeatCount-- > 0)

            sb.Append(value);

        return sb.ToString();

    }



    /// <summary>

    /// 	Tests whether the contents of a string is a numeric value

    /// </summary>

    /// <param name = "value">String to check</param>

    /// <returns>

    /// 	Boolean indicating whether or not the string contents are numeric

    /// </returns>

    /// <remarks>

    /// 	Contributed by Kenneth Scott

    /// </remarks>

    public static bool IsNumeric(this string value)

    {

        float output;

        return float.TryParse(value, out output);

    }



    #region Extract







    /// <summary>

    /// 	Extracts all digits from a string.

    /// </summary>

    /// <param name = "value">String containing digits to extract</param>

    /// <returns>

    /// 	All digits contained within the input string

    /// </returns>

    /// <remarks>

    /// 	Contributed by Kenneth Scott

    /// </remarks>



    public static string ExtractDigits(this string value)

    {

        return value.Where(Char.IsDigit).Aggregate(new StringBuilder(value.Length), (sb, c) => sb.Append(c)).ToString();

    }







    #endregion



    /// <summary>

    /// 	Concatenates the specified string value with the passed additional strings.

    /// </summary>

    /// <param name = "value">The original value.</param>

    /// <param name = "values">The additional string values to be concatenated.</param>

    /// <returns>The concatenated string.</returns>

    public static string ConcatWith(this string value, params string[] values)

    {

        return string.Concat(value, string.Concat(values));

    }



    /// <summary>

    /// 	Convert the provided string to a Guid value.

    /// </summary>

    /// <param name = "value">The original string value.</param>

    /// <returns>The Guid</returns>

    public static Guid ToGuid(this string value)

    {

        return new Guid(value);

    }



    /// <summary>

    /// 	Convert the provided string to a Guid value and returns Guid.Empty if conversion fails.

    /// </summary>

    /// <param name = "value">The original string value.</param>

    /// <returns>The Guid</returns>

    public static Guid ToGuidSave(this string value)

    {

        return value.ToGuidSave(Guid.Empty);

    }



    /// <summary>

    /// 	Convert the provided string to a Guid value and returns the provided default value if the conversion fails.

    /// </summary>

    /// <param name = "value">The original string value.</param>

    /// <param name = "defaultValue">The default value.</param>

    /// <returns>The Guid</returns>

    public static Guid ToGuidSave(this string value, Guid defaultValue)

    {

        if (value.IsEmpty())

            return defaultValue;



        try

        {

            return value.ToGuid();

        }

        catch { }



        return defaultValue;

    }



    /// <summary>

    /// 	Gets the string before the given string parameter.

    /// </summary>

    /// <param name = "value">The default value.</param>

    /// <param name = "x">The given string parameter.</param>

    /// <returns></returns>

    /// <remarks>Unlike GetBetween and GetAfter, this does not Trim the result.</remarks>

    public static string GetBefore(this string value, string x)

    {

        var xPos = value.IndexOf(x);

        return xPos == -1 ? String.Empty : value.Substring(0, xPos);

    }



    /// <summary>

    /// 	Gets the string between the given string parameters.

    /// </summary>

    /// <param name = "value">The source value.</param>

    /// <param name = "x">The left string sentinel.</param>

    /// <param name = "y">The right string sentinel</param>

    /// <returns></returns>

    /// <remarks>Unlike GetBefore, this method trims the result</remarks>

    public static string GetBetween(this string value, string x, string y)

    {

        var xPos = value.IndexOf(x);

        var yPos = value.LastIndexOf(y);



        if (xPos == -1 || xPos == -1)

            return String.Empty;



        var startIndex = xPos + x.Length;

        return startIndex >= yPos ? String.Empty : value.Substring(startIndex, yPos - startIndex).Trim();

    }



    /// <summary>

    /// 	Gets the string after the given string parameter.

    /// </summary>

    /// <param name = "value">The default value.</param>

    /// <param name = "x">The given string parameter.</param>

    /// <returns></returns>

    /// <remarks>Unlike GetBefore, this method trims the result</remarks>

    public static string GetAfter(this string value, string x)

    {

        var xPos = value.LastIndexOf(x);



        if (xPos == -1)

            return String.Empty;



        var startIndex = xPos + x.Length;

        return startIndex >= value.Length ? String.Empty : value.Substring(startIndex).Trim();

    }



    /// <summary>

    /// 	A generic version of System.String.Join()

    /// </summary>

    /// <typeparam name = "T">

    /// 	The type of the array to join

    /// </typeparam>

    /// <param name = "separator">

    /// 	The separator to appear between each element

    /// </param>

    /// <param name = "value">

    /// 	An array of values

    /// </param>

    /// <returns>

    /// 	The join.

    /// </returns>

    /// <remarks>

    /// 	Contributed by Michael T, http://about.me/MichaelTran

    /// </remarks>

    public static string Join<T>(string separator, T[] value)

    {

        if (value == null || value.Length == 0)

            return string.Empty;

        if (separator == null)

            separator = string.Empty;

        Converter<T, string> converter = o => o.ToString();

        return string.Join(separator, Array.ConvertAll(value, converter));

    }



    /// <summary>

    /// 	Remove any instance of the given character from the current string.

    /// </summary>

    /// <param name = "value">

    /// 	The input.

    /// </param>

    /// <param name = "removeCharc">

    /// 	The remove char.

    /// </param>

    /// <remarks>

    /// 	Contributed by Michael T, http://about.me/MichaelTran

    /// </remarks>

    public static string Remove(this string value, params char[] removeCharc)

    {

        var result = value;

        if (!string.IsNullOrEmpty(result) && removeCharc != null)

            Array.ForEach(removeCharc, c => result = result.Remove(c.ToString()));



        return result;



    }



    /// <summary>

    /// Remove any instance of the given string pattern from the current string.

    /// </summary>

    /// <param name="value">The input.</param>

    /// <param name="strings">The strings.</param>

    /// <returns></returns>

    /// <remarks>

    /// Contributed by Michael T, http://about.me/MichaelTran

    /// </remarks>

    public static string Remove(this string value, params string[] strings)

    {

        return strings.Aggregate(value, (current, c) => current.Replace(c, string.Empty));

    }



    /// <summary>Finds out if the specified string contains null, empty or consists only of white-space characters</summary>

    /// <param name = "value">The input string</param>

    public static bool IsEmptyOrWhiteSpace(this string value)

    {

        return (value.IsEmpty() || value.All(t => char.IsWhiteSpace(t)));

    }



    /// <summary>Determines whether the specified string is not null, empty or consists only of white-space characters</summary>

    /// <param name = "value">The string value to check</param>

    public static bool IsNotEmptyOrWhiteSpace(this string value)

    {

        return (value.IsEmptyOrWhiteSpace() == false);

    }



    /// <summary>Checks whether the string is null, empty or consists only of white-space characters and returns a default value in case</summary>

    /// <param name = "value">The string to check</param>

    /// <param name = "defaultValue">The default value</param>

    /// <returns>Either the string or the default value</returns>

    public static string IfEmptyOrWhiteSpace(this string value, string defaultValue)

    {

        return (value.IsEmptyOrWhiteSpace() ? defaultValue : value);

    }



    /// <summary>Uppercase First Letter</summary>

    /// <param name = "value">The string value to process</param>

    public static string ToUpperFirstLetter(this string value)

    {

        if (value.IsEmptyOrWhiteSpace()) return string.Empty;



        char[] valueChars = value.ToCharArray();

        valueChars[0] = char.ToUpper(valueChars[0]);



        return new string(valueChars);

    }



    /// <summary>

    /// Returns the left part of the string.

    /// </summary>

    /// <param name="value">The original string.</param>

    /// <param name="characterCount">The character count to be returned.</param>

    /// <returns>The left part</returns>

    public static string Left(this string value, int characterCount)

    {

        if (value == null)

            throw new ArgumentNullException("value");

        if (characterCount >= value.Length)

            throw new ArgumentOutOfRangeException("characterCount", characterCount, "characterCount must be less than length of string");

        return value.Substring(0, characterCount);

    }



    /// <summary>

    /// Returns the Right part of the string.

    /// </summary>

    /// <param name="value">The original string.</param>

    /// <param name="characterCount">The character count to be returned.</param>

    /// <returns>The right part</returns>

    public static string Right(this string value, int characterCount)

    {

        if (value == null)

            throw new ArgumentNullException("value");

        if (characterCount >= value.Length)

            throw new ArgumentOutOfRangeException("characterCount", characterCount, "characterCount must be less than length of string");

        return value.Substring(value.Length - characterCount);

    }



    /// <summary>Returns the right part of the string from index.</summary>

    /// <param name="value">The original value.</param>

    /// <param name="index">The start index for substringing.</param>

    /// <returns>The right part.</returns>

    public static string SubstringFrom(this string value, int index)

    {

        return index < 0 ? value : value.Substring(index, value.Length - index);

    }



    //todo: xml documentation requires

    //todo: unit test required

    public static byte[] GetBytes(this string data)

    {

        return Encoding.Default.GetBytes(data);

    }



    public static byte[] GetBytes(this string data, Encoding encoding)

    {

        return encoding.GetBytes(data);

    }



    /// <summary>

    /// Returns true if strings are equals, without consideration to case (<see cref="StringComparison.InvariantCultureIgnoreCase"/>)

    /// </summary>

    public static bool EquivalentTo(this string s, string whateverCaseString)

    {

        return string.Equals(s, whateverCaseString, StringComparison.InvariantCultureIgnoreCase);

    }



    /// <summary>

    /// Replace all values in string

    /// </summary>

    /// <param name="value">The input string.</param>

    /// <param name="oldValues">List of old values, which must be replaced</param>

    /// <param name="replacePredicate">Function for replacement old values</param>

    /// <returns>Returns new string with the replaced values</returns>

    /// <example>

    /// 	<code>

    ///         var str = "White Red Blue Green Yellow Black Gray";

    ///         var achromaticColors = new[] {"White", "Black", "Gray"};

    ///         str = str.ReplaceAll(achromaticColors, v => "[" + v + "]");

    ///         // str == "[White] Red Blue Green Yellow [Black] [Gray]"

    /// 	</code>

    /// </example>

    /// <remarks>

    /// 	Contributed by nagits, http://about.me/AlekseyNagovitsyn

    /// </remarks>

    public static string ReplaceAll(this string value, IEnumerable<string> oldValues, Func<string, string> replacePredicate)

    {

        var sbStr = new StringBuilder(value);

        foreach (var oldValue in oldValues)

        {

            var newValue = replacePredicate(oldValue);

            sbStr.Replace(oldValue, newValue);

        }



        return sbStr.ToString();

    }



    /// <summary>

    /// Replace all values in string

    /// </summary>

    /// <param name="value">The input string.</param>

    /// <param name="oldValues">List of old values, which must be replaced</param>

    /// <param name="newValue">New value for all old values</param>

    /// <returns>Returns new string with the replaced values</returns>

    /// <example>

    /// 	<code>

    ///         var str = "White Red Blue Green Yellow Black Gray";

    ///         var achromaticColors = new[] {"White", "Black", "Gray"};

    ///         str = str.ReplaceAll(achromaticColors, "[AchromaticColor]");

    ///         // str == "[AchromaticColor] Red Blue Green Yellow [AchromaticColor] [AchromaticColor]"

    /// 	</code>

    /// </example>

    /// <remarks>

    /// 	Contributed by nagits, http://about.me/AlekseyNagovitsyn

    /// </remarks>

    public static string ReplaceAll(this string value, IEnumerable<string> oldValues, string newValue)

    {

        var sbStr = new StringBuilder(value);

        foreach (var oldValue in oldValues)

            sbStr.Replace(oldValue, newValue);



        return sbStr.ToString();

    }



    /// <summary>

    /// Replace all values in string

    /// </summary>

    /// <param name="value">The input string.</param>

    /// <param name="oldValues">List of old values, which must be replaced</param>

    /// <param name="newValues">List of new values</param>

    /// <returns>Returns new string with the replaced values</returns>

    /// <example>

    /// 	<code>

    ///         var str = "White Red Blue Green Yellow Black Gray";

    ///         var achromaticColors = new[] {"White", "Black", "Gray"};

    ///         var exquisiteColors = new[] {"FloralWhite", "Bistre", "DavyGrey"};

    ///         str = str.ReplaceAll(achromaticColors, exquisiteColors);

    ///         // str == "FloralWhite Red Blue Green Yellow Bistre DavyGrey"

    /// 	</code>

    /// </example>

    /// <remarks>

    /// 	Contributed by nagits, http://about.me/AlekseyNagovitsyn

    /// </remarks> 

    public static string ReplaceAll(this string value, IEnumerable<string> oldValues, IEnumerable<string> newValues)

    {

        var sbStr = new StringBuilder(value);

        var newValueEnum = newValues.GetEnumerator();

        foreach (var old in oldValues)

        {

            if (!newValueEnum.MoveNext())

                throw new ArgumentOutOfRangeException("newValues", "newValues sequence is shorter than oldValues sequence");

            sbStr.Replace(old, newValueEnum.Current);

        }

        if (newValueEnum.MoveNext())

            throw new ArgumentOutOfRangeException("newValues", "newValues sequence is longer than oldValues sequence");



        return sbStr.ToString();

    }



    #endregion

    #region Regex based extension methods



    /// <summary>

    /// 	Uses regular expressions to determine if the string matches to a given regex pattern.

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <param name = "regexPattern">The regular expression pattern.</param>

    /// <returns>

    /// 	<c>true</c> if the value is matching to the specified pattern; otherwise, <c>false</c>.

    /// </returns>

    /// <example>

    /// 	<code>

    /// 		var s = "12345";

    /// 		var isMatching = s.IsMatchingTo(@"^\d+$");

    /// 	</code>

    /// </example>

    public static bool IsMatchingTo(this string value, string regexPattern)

    {

        return IsMatchingTo(value, regexPattern, RegexOptions.None);

    }



    /// <summary>

    /// 	Uses regular expressions to determine if the string matches to a given regex pattern.

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <param name = "regexPattern">The regular expression pattern.</param>

    /// <param name = "options">The regular expression options.</param>

    /// <returns>

    /// 	<c>true</c> if the value is matching to the specified pattern; otherwise, <c>false</c>.

    /// </returns>

    /// <example>

    /// 	<code>

    /// 		var s = "12345";

    /// 		var isMatching = s.IsMatchingTo(@"^\d+$");

    /// 	</code>

    /// </example>

    public static bool IsMatchingTo(this string value, string regexPattern, RegexOptions options)

    {

        return Regex.IsMatch(value, regexPattern, options);

    }



    /// <summary>

    /// 	Uses regular expressions to replace parts of a string.

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <param name = "regexPattern">The regular expression pattern.</param>

    /// <param name = "replaceValue">The replacement value.</param>

    /// <returns>The newly created string</returns>

    /// <example>

    /// 	<code>

    /// 		var s = "12345";

    /// 		var replaced = s.ReplaceWith(@"\d", m => string.Concat(" -", m.Value, "- "));

    /// 	</code>

    /// </example>

    public static string ReplaceWith(this string value, string regexPattern, string replaceValue)

    {

        return ReplaceWith(value, regexPattern, replaceValue, RegexOptions.None);

    }



    /// <summary>

    /// 	Uses regular expressions to replace parts of a string.

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <param name = "regexPattern">The regular expression pattern.</param>

    /// <param name = "replaceValue">The replacement value.</param>

    /// <param name = "options">The regular expression options.</param>

    /// <returns>The newly created string</returns>

    /// <example>

    /// 	<code>

    /// 		var s = "12345";

    /// 		var replaced = s.ReplaceWith(@"\d", m => string.Concat(" -", m.Value, "- "));

    /// 	</code>

    /// </example>

    public static string ReplaceWith(this string value, string regexPattern, string replaceValue, RegexOptions options)

    {

        return Regex.Replace(value, regexPattern, replaceValue, options);

    }



    /// <summary>

    /// 	Uses regular expressions to replace parts of a string.

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <param name = "regexPattern">The regular expression pattern.</param>

    /// <param name = "evaluator">The replacement method / lambda expression.</param>

    /// <returns>The newly created string</returns>

    /// <example>

    /// 	<code>

    /// 		var s = "12345";

    /// 		var replaced = s.ReplaceWith(@"\d", m => string.Concat(" -", m.Value, "- "));

    /// 	</code>

    /// </example>

    public static string ReplaceWith(this string value, string regexPattern, MatchEvaluator evaluator)

    {

        return ReplaceWith(value, regexPattern, RegexOptions.None, evaluator);

    }



    /// <summary>

    /// 	Uses regular expressions to replace parts of a string.

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <param name = "regexPattern">The regular expression pattern.</param>

    /// <param name = "options">The regular expression options.</param>

    /// <param name = "evaluator">The replacement method / lambda expression.</param>

    /// <returns>The newly created string</returns>

    /// <example>

    /// 	<code>

    /// 		var s = "12345";

    /// 		var replaced = s.ReplaceWith(@"\d", m => string.Concat(" -", m.Value, "- "));

    /// 	</code>

    /// </example>

    public static string ReplaceWith(this string value, string regexPattern, RegexOptions options, MatchEvaluator evaluator)

    {

        return Regex.Replace(value, regexPattern, evaluator, options);

    }



    /// <summary>

    /// 	Uses regular expressions to determine all matches of a given regex pattern.

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <param name = "regexPattern">The regular expression pattern.</param>

    /// <returns>A collection of all matches</returns>

    public static MatchCollection GetMatches(this string value, string regexPattern)

    {

        return GetMatches(value, regexPattern, RegexOptions.None);

    }



    /// <summary>

    /// 	Uses regular expressions to determine all matches of a given regex pattern.

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <param name = "regexPattern">The regular expression pattern.</param>

    /// <param name = "options">The regular expression options.</param>

    /// <returns>A collection of all matches</returns>

    public static MatchCollection GetMatches(this string value, string regexPattern, RegexOptions options)

    {

        return Regex.Matches(value, regexPattern, options);

    }



    /// <summary>

    /// 	Uses regular expressions to determine all matches of a given regex pattern and returns them as string enumeration.

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <param name = "regexPattern">The regular expression pattern.</param>

    /// <returns>An enumeration of matching strings</returns>

    /// <example>

    /// 	<code>

    /// 		var s = "12345";

    /// 		foreach(var number in s.GetMatchingValues(@"\d")) {

    /// 		Console.WriteLine(number);

    /// 		}

    /// 	</code>

    /// </example>

    public static IEnumerable<string> GetMatchingValues(this string value, string regexPattern)

    {

        return GetMatchingValues(value, regexPattern, RegexOptions.None);

    }



    /// <summary>

    /// 	Uses regular expressions to determine all matches of a given regex pattern and returns them as string enumeration.

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <param name = "regexPattern">The regular expression pattern.</param>

    /// <param name = "options">The regular expression options.</param>

    /// <returns>An enumeration of matching strings</returns>

    /// <example>

    /// 	<code>

    /// 		var s = "12345";

    /// 		foreach(var number in s.GetMatchingValues(@"\d")) {

    /// 		Console.WriteLine(number);

    /// 		}

    /// 	</code>

    /// </example>

    public static IEnumerable<string> GetMatchingValues(this string value, string regexPattern, RegexOptions options)

    {

        foreach (Match match in GetMatches(value, regexPattern, options))

        {

            if (match.Success) yield return match.Value;

        }

    }



    /// <summary>

    /// 	Uses regular expressions to split a string into parts.

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <param name = "regexPattern">The regular expression pattern.</param>

    /// <returns>The splitted string array</returns>

    public static string[] Split(this string value, string regexPattern)

    {

        return value.Split(regexPattern, RegexOptions.None);

    }



    /// <summary>

    /// 	Uses regular expressions to split a string into parts.

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <param name = "regexPattern">The regular expression pattern.</param>

    /// <param name = "options">The regular expression options.</param>

    /// <returns>The splitted string array</returns>

    public static string[] Split(this string value, string regexPattern, RegexOptions options)

    {

        return Regex.Split(value, regexPattern, options);

    }



    /// <summary>

    /// 	Splits the given string into words and returns a string array.

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <returns>The splitted string array</returns>

    public static string[] GetWords(this string value)

    {

        return value.Split(@"\W");

    }



    /// <summary>

    /// 	Gets the nth "word" of a given string, where "words" are substrings separated by a given separator

    /// </summary>

    /// <param name = "value">The string from which the word should be retrieved.</param>

    /// <param name = "index">Index of the word (0-based).</param>

    /// <returns>

    /// 	The word at position n of the string.

    /// 	Trying to retrieve a word at a position lower than 0 or at a position where no word exists results in an exception.

    /// </returns>

    /// <remarks>

    /// 	Originally contributed by MMathews

    /// </remarks>

    public static string GetWordByIndex(this string value, int index)

    {

        var words = value.GetWords();



        if ((index < 0) || (index > words.Length - 1))

            throw new IndexOutOfRangeException("The word number is out of range.");



        return words[index];

    }



    /// <summary>

    /// Removed all special characters from the string.

    /// </summary>

    /// <param name="value">The input string.</param>

    /// <returns>The adjusted string.</returns>

    /// <remarks>

    /// 	Contributed by Michael T, http://about.me/MichaelTran, James C, http://www.noveltheory.com

    /// 	This implementation is roughly equal to the original in speed, and should be more robust, internationally.

    /// </remarks>

    public static string RemoveAllSpecialCharacters(this string value)

    {

        var sb = new StringBuilder(value.Length);

        foreach (var c in value.Where(c => Char.IsLetterOrDigit(c)))

            sb.Append(c);

        return sb.ToString();

    }





    /// <summary>

    /// Add space on every upper character

    /// </summary>

    /// <param name="value">The input string.</param>

    /// <returns>The adjusted string.</returns>

    /// <remarks>

    /// 	Contributed by Michael T, http://about.me/MichaelTran

    /// </remarks>

    public static string SpaceOnUpper(this string value)

    {

        return Regex.Replace(value, "([A-Z])(?=[a-z])|(?<=[a-z])([A-Z]|[0-9]+)", " $1$2").TrimStart();

    }



    #region ExtractArguments extension



    /// <summary>

    /// Options to match the template with the original string

    /// </summary>

    public enum ComparsionTemplateOptions

    {

        /// <summary>

        /// Free template comparsion

        /// </summary>

        Default,



        /// <summary>

        /// Template compared from beginning of input string

        /// </summary>

        FromStart,



        /// <summary>

        /// Template compared with the end of input string

        /// </summary>

        AtTheEnd,



        /// <summary>

        /// Template compared whole with input string

        /// </summary>

        Whole,

    }



    private const RegexOptions _defaultRegexOptions = RegexOptions.None;

    private const ComparsionTemplateOptions _defaultComparsionTemplateOptions = ComparsionTemplateOptions.Default;

    private static readonly string[] _reservedRegexOperators = new[] { @"\", "^", "$", "*", "+", "?", ".", "(", ")" };



    private static string GetRegexPattern(string template, ComparsionTemplateOptions compareTemplateOptions)

    {

        template = template.ReplaceAll(_reservedRegexOperators, v => @"\" + v);



        bool comparingFromStart = compareTemplateOptions == ComparsionTemplateOptions.FromStart ||

            compareTemplateOptions == ComparsionTemplateOptions.Whole;

        bool comparingAtTheEnd = compareTemplateOptions == ComparsionTemplateOptions.AtTheEnd ||

            compareTemplateOptions == ComparsionTemplateOptions.Whole;

        var pattern = new StringBuilder();



        if (comparingFromStart)

            pattern.Append("^");



        pattern.Append(Regex.Replace(template, @"\{[0-9]+\}",

                                     delegate (Match m)

                                     {

                                         var argNum = m.ToString().Replace("{", "").Replace("}", "");

                                         return String.Format("(?<{0}>.*?)", int.Parse(argNum) + 1);

                                     }

        ));



        if (comparingAtTheEnd || (template.LastOrDefault() == '}' && compareTemplateOptions == ComparsionTemplateOptions.Default))

            pattern.Append("$");



        return pattern.ToString();

    }



    /// <summary>

    /// Extract arguments from string by template

    /// </summary>

    /// <param name="value">The input string. For example, "My name is Aleksey".</param>

    /// <param name="template">Template with arguments in the format {№}. For example, "My name is {0} {1}.".</param>

    /// <param name="compareTemplateOptions">Template options for compare with input string.</param>

    /// <param name="regexOptions">Regex options.</param>

    /// <returns>Returns parsed arguments.</returns>

    /// <example>

    /// 	<code>

    /// 		var str = "My name is Aleksey Nagovitsyn. I'm from Russia.";

    /// 		var args = str.ExtractArguments(@"My name is {1} {0}. I'm from {2}.");

    ///         // args[i] is [Nagovitsyn, Aleksey, Russia]

    /// 	</code>

    /// </example>

    /// <remarks>

    /// 	Contributed by nagits, http://about.me/AlekseyNagovitsyn

    /// </remarks>

    public static IEnumerable<string> ExtractArguments(this string value, string template,

                                                       ComparsionTemplateOptions compareTemplateOptions = _defaultComparsionTemplateOptions,

                                                       RegexOptions regexOptions = _defaultRegexOptions)

    {

        return ExtractGroupArguments(value, template, compareTemplateOptions, regexOptions).Select(g => g.Value);

    }



    /// <summary>

    /// Extract arguments as regex groups from string by template

    /// </summary>

    /// <param name="value">The input string. For example, "My name is Aleksey".</param>

    /// <param name="template">Template with arguments in the format {№}. For example, "My name is {0} {1}.".</param>

    /// <param name="compareTemplateOptions">Template options for compare with input string.</param>

    /// <param name="regexOptions">Regex options.</param>

    /// <returns>Returns parsed arguments as regex groups.</returns>

    /// <example>

    /// 	<code>

    /// 		var str = "My name is Aleksey Nagovitsyn. I'm from Russia.";

    /// 		var groupArgs = str.ExtractGroupArguments(@"My name is {1} {0}. I'm from {2}.");

    ///         // groupArgs[i].Value is [Nagovitsyn, Aleksey, Russia]

    /// 	</code>

    /// </example>

    /// <remarks>

    /// 	Contributed by nagits, http://about.me/AlekseyNagovitsyn

    /// </remarks>

    public static IEnumerable<Group> ExtractGroupArguments(this string value, string template,

                                                           ComparsionTemplateOptions compareTemplateOptions = _defaultComparsionTemplateOptions,

                                                           RegexOptions regexOptions = _defaultRegexOptions)

    {

        var pattern = GetRegexPattern(template, compareTemplateOptions);

        var regex = new Regex(pattern, regexOptions);

        var match = regex.Match(value);



        return Enumerable.Cast<Group>(match.Groups).Skip(1);

    }



    #endregion ExtractArguments extension



    #endregion

    #region Bytes & Base64



    /// <summary>

    /// 	Converts the string to a byte-array using the default encoding

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <returns>The created byte array</returns>

    public static byte[] ToBytes(this string value)

    {

        return value.ToBytes(null);

    }



    /// <summary>

    /// 	Converts the string to a byte-array using the supplied encoding

    /// </summary>

    /// <param name = "value">The input string.</param>

    /// <param name = "encoding">The encoding to be used.</param>

    /// <returns>The created byte array</returns>

    /// <example>

    /// 	<code>

    /// 		var value = "Hello World";

    /// 		var ansiBytes = value.ToBytes(Encoding.GetEncoding(1252)); // 1252 = ANSI

    /// 		var utf8Bytes = value.ToBytes(Encoding.UTF8);

    /// 	</code>

    /// </example>

    public static byte[] ToBytes(this string value, Encoding encoding)

    {

        encoding = (encoding ?? Encoding.Default);

        return encoding.GetBytes(value);

    }



    /// <summary>

    /// 	Encodes the input value to a Base64 string using the default encoding.

    /// </summary>

    /// <param name = "value">The input value.</param>

    /// <returns>The Base 64 encoded string</returns>

    public static string EncodeBase64(this string value)

    {

        return value.EncodeBase64(null);

    }



    /// <summary>

    /// 	Encodes the input value to a Base64 string using the supplied encoding.

    /// </summary>

    /// <param name = "value">The input value.</param>

    /// <param name = "encoding">The encoding.</param>

    /// <returns>The Base 64 encoded string</returns>

    public static string EncodeBase64(this string value, Encoding encoding)

    {

        encoding = (encoding ?? Encoding.UTF8);

        var bytes = encoding.GetBytes(value);

        return Convert.ToBase64String(bytes);

    }



    /// <summary>

    /// 	Decodes a Base 64 encoded value to a string using the default encoding.

    /// </summary>

    /// <param name = "encodedValue">The Base 64 encoded value.</param>

    /// <returns>The decoded string</returns>

    public static string DecodeBase64(this string encodedValue)

    {

        return encodedValue.DecodeBase64(null);

    }



    /// <summary>

    /// 	Decodes a Base 64 encoded value to a string using the supplied encoding.

    /// </summary>

    /// <param name = "encodedValue">The Base 64 encoded value.</param>

    /// <param name = "encoding">The encoding.</param>

    /// <returns>The decoded string</returns>

    public static string DecodeBase64(this string encodedValue, Encoding encoding)

    {

        encoding = (encoding ?? Encoding.UTF8);

        var bytes = Convert.FromBase64String(encodedValue);

        return encoding.GetString(bytes);

    }



    #endregion



    #region String to Enum



    /// <summary>

    ///     Parse a string to a enum item if that string exists in the enum otherwise return the default enum item.

    /// </summary>

    /// <typeparam name="TEnum">The Enum type.</typeparam>

    /// <param name="dataToMatch">The data will use to convert into give enum</param>

    /// <param name="ignorecase">Whether the enum parser will ignore the given data's case or not.</param>

    /// <returns>Converted enum.</returns>

    /// <example>

    /// 	<code>

    /// 		public enum EnumTwo {  None, One,}

    /// 		object[] items = new object[] { "One".ParseStringToEnum<EnumTwo>(), "Two".ParseStringToEnum<EnumTwo>() };

    /// 	</code>

    /// </example>

    /// <remarks>

    /// 	Contributed by Mohammad Rahman, http://mohammad-rahman.blogspot.com/

    /// </remarks>

    public static TEnum ParseStringToEnum<TEnum>(this string dataToMatch, bool ignorecase = default(bool))

        where TEnum : struct

    {

        return dataToMatch.IsItemInEnum<TEnum>()() ? default(TEnum) : (TEnum)Enum.Parse(typeof(TEnum), dataToMatch, default(bool));

    }



    /// <summary>

    ///     To check whether the data is defined in the given enum.

    /// </summary>

    /// <typeparam name="TEnum">The enum will use to check, the data defined.</typeparam>

    /// <param name="dataToCheck">To match against enum.</param>

    /// <returns>Anonoymous method for the condition.</returns>

    /// <remarks>

    /// 	Contributed by Mohammad Rahman, http://mohammad-rahman.blogspot.com/

    /// </remarks>

    public static Func<bool> IsItemInEnum<TEnum>(this string dataToCheck)

        where TEnum : struct

    {

        return () => { return string.IsNullOrEmpty(dataToCheck) || !Enum.IsDefined(typeof(TEnum), dataToCheck); };

    }



    #endregion



    private static bool StringContainsEquivalence(string inputValue, string comparisonValue)

    {

        return (inputValue.IsNotEmptyOrWhiteSpace() && inputValue.Contains(comparisonValue, StringComparison.InvariantCultureIgnoreCase));

    }



    private static bool BothStringsAreEmpty(string inputValue, string comparisonValue)

    {

        return (inputValue.IsEmptyOrWhiteSpace() && comparisonValue.IsEmptyOrWhiteSpace());

    }



    /// <summary>

    /// Return the string with the leftmost number_of_characters characters removed.

    /// </summary>

    /// <param name="str">The string being extended</param>

    /// <param name="number_of_characters">The number of characters to remove.</param>

    /// <returns></returns>

    /// <remarks></remarks>

    public static String RemoveLeft(this String str, int number_of_characters)

    {

        if (str.Length <= number_of_characters) return "";

        return str.Substring(number_of_characters);

    }



    /// <summary>

    /// Return the string with the rightmost number_of_characters characters removed.

    /// </summary>

    /// <param name="str">The string being extended</param>

    /// <param name="number_of_characters">The number of characters to remove.</param>

    /// <returns></returns>

    /// <remarks></remarks>

    public static String RemoveRight(this String str, int number_of_characters)

    {

        if (str.Length <= number_of_characters) return "";

        return str.Substring(0, str.Length - number_of_characters);

    }



    /// <summary>

    /// Encrypt this string into a byte array.

    /// </summary>

    /// <param name="plain_text">The string being extended and that will be encrypted.</param>

    /// <param name="password">The password to use then encrypting the string.</param>

    /// <returns></returns>

    /// <remarks></remarks>

    public static byte[] EncryptToByteArray(this String plain_text, String password)

    {

        var ascii_encoder = new ASCIIEncoding();

        byte[] plain_bytes = ascii_encoder.GetBytes(plain_text);

        return CryptBytes(password, plain_bytes, true);

    }



    /// <summary>

    /// Decrypt the encryption stored in this byte array.

    /// </summary>

    /// <param name="encrypted_bytes">The byte array to decrypt.</param>

    /// <param name="password">The password to use when decrypting.</param>

    /// <returns></returns>

    /// <remarks></remarks>

    public static String DecryptFromByteArray(this byte[] encrypted_bytes, String password)

    {

        byte[] decrypted_bytes = CryptBytes(password, encrypted_bytes, false);

        var ascii_encoder = new ASCIIEncoding();

        return new String(ascii_encoder.GetChars(decrypted_bytes));

    }



    /// <summary>

    /// Encrypt this string and return the result as a string of hexadecimal characters.

    /// </summary>

    /// <param name="plain_text">The string being extended and that will be encrypted.</param>

    /// <param name="password">The password to use then encrypting the string.</param>

    /// <returns></returns>

    /// <remarks></remarks>

    public static String EncryptToString(this String plain_text, String password)

    {

        return plain_text.EncryptToByteArray(password).BytesToHexString();

    }



    /// <summary>

    /// Decrypt the encryption stored in this string of hexadecimal values.

    /// </summary>

    /// <param name="encrypted_bytes_string">The hexadecimal string to decrypt.</param>

    /// <param name="password">The password to use then encrypting the string.</param>

    /// <returns></returns>

    /// <remarks></remarks>

    public static String DecryptFromString(this String encrypted_bytes_string, String password)

    {

        return encrypted_bytes_string.HexStringToBytes().DecryptFromByteArray(password);

    }



    /// <summary>

    /// Encrypt or decrypt a byte array using the TripleDESCryptoServiceProvider crypto provider and Rfc2898DeriveBytes to build the key and initialization vector.

    /// </summary>

    /// <param name="password">The password string to use in encrypting or decrypting.</param>

    /// <param name="in_bytes">The array of bytes to encrypt.</param>

    /// <param name="encrypt">True to encrypt, False to decrypt.</param>

    /// <returns></returns>

    /// <remarks></remarks>

    private static byte[] CryptBytes(String password, byte[] in_bytes, bool encrypt)

    {

        // Make a triple DES service provider.

        var des_provider = new TripleDESCryptoServiceProvider();



        // Find a valid key size for this provider.

        int key_size_bits = 0;

        for (int i = 1024; i >= 1; i--)

        {

            if (des_provider.ValidKeySize(i))

            {

                key_size_bits = i;

                break;

            }

        }



        // Get the block size for this provider.

        int block_size_bits = des_provider.BlockSize;



        // Generate the key and initialization vector.

        byte[] key = null;

        byte[] iv = null;

        byte[] salt = { 0x10, 0x20, 0x12, 0x23, 0x37, 0xA4, 0xC5, 0xA6, 0xF1, 0xF0, 0xEE, 0x21, 0x22, 0x45 };

        MakeKeyAndIV(password, salt, key_size_bits, block_size_bits, ref key, ref iv);



        // Make the encryptor or decryptor.

        ICryptoTransform crypto_transform = encrypt

            ? des_provider.CreateEncryptor(key, iv)

                : des_provider.CreateDecryptor(key, iv);



        // Create the output stream.

        var out_stream = new MemoryStream();



        // Attach a crypto stream to the output stream.

        var crypto_stream = new CryptoStream(out_stream,

                                             crypto_transform, CryptoStreamMode.Write);



        // Write the bytes into the CryptoStream.

        crypto_stream.Write(in_bytes, 0, in_bytes.Length);

        try

        {

            crypto_stream.FlushFinalBlock();

        }

        catch (CryptographicException)

        {

            // Ignore this one. The password is bad.

        }



        // Save the result.

        byte[] result = out_stream.ToArray();



        // Close the stream.

        try

        {

            crypto_stream.Close();

        }

        catch (CryptographicException)

        {

            // Ignore this one. The password is bad.

        }

        out_stream.Close();



        return result;

    }



    /// <summary>

    /// Use the password to generate key bytes and an initialization vector with Rfc2898DeriveBytes.

    /// </summary>

    /// <param name="password">The input password to use in generating the bytes.</param>

    /// <param name="salt">The input salt bytes to use in generating the bytes.</param>

    /// <param name="key_size_bits">The input size of the key to generate.</param>

    /// <param name="block_size_bits">The input block size used by the crypto provider.</param>

    /// <param name="key">The output key bytes to generate.</param>

    /// <param name="iv">The output initialization vector to generate.</param>

    /// <remarks></remarks>

    private static void MakeKeyAndIV(String password, byte[] salt, int key_size_bits, int block_size_bits,

                                     ref byte[] key, ref byte[] iv)

    {

        var derive_bytes =

            new Rfc2898DeriveBytes(password, salt, 1234);



        key = derive_bytes.GetBytes(key_size_bits / 8);

        iv = derive_bytes.GetBytes(block_size_bits / 8);

    }



    /// <summary>

    /// Convert a byte array into a hexadecimal string representation.

    /// </summary>

    /// <param name="bytes"></param>

    /// <returns></returns>

    /// <remarks></remarks>

    public static String BytesToHexString(this byte[] bytes)

    {

        String result = "";

        foreach (byte b in bytes)

        {

            result += " " + b.ToString("X").PadLeft(2, '0');

        }

        if (result.Length > 0) result = result.Substring(1);

        return result;

    }



    /// <summary>

    /// Convert this string containing hexadecimal into a byte array.

    /// </summary>

    /// <param name="str">The hexadecimal string to convert.</param>

    /// <returns></returns>

    /// <remarks></remarks>

    public static byte[] HexStringToBytes(this String str)

    {

        str = str.Replace(" ", "");

        int max_byte = str.Length / 2 - 1;

        var bytes = new byte[max_byte + 1];

        for (int i = 0; i <= max_byte; i++)

        {

            bytes[i] = byte.Parse(str.Substring(2 * i, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

        }



        return bytes;

    }



    /// <summary>

    /// Returns a default value if the string is null or empty.

    /// </summary>

    /// <param name="s">Original String</param>

    /// <param name="defaultValue">The default value.</param>

    /// <returns></returns>

    public static string DefaultIfNullOrEmpty(this string s, string defaultValue)

    {

        return String.IsNullOrEmpty(s) ? defaultValue : s;

    }



    /// <summary>

    /// Throws an <see cref="System.ArgumentException"/> if the string value is empty.

    /// </summary>

    /// <param name="obj">The value to test.</param>

    /// <param name="message">The message to display if the value is null.</param>

    /// <param name="name">The name of the parameter being tested.</param>

    public static string ExceptionIfNullOrEmpty(this string obj, string message, string name)

    {

        if (String.IsNullOrEmpty(obj))

            throw new ArgumentException(message, name);

        return obj;

    }



    /// <summary>

    /// Joins  the values of a string array if the values are not null or empty.

    /// </summary>

    /// <param name="objs">The string array used for joining.</param>

    /// <param name="separator">The separator to use in the joined string.</param>

    /// <returns></returns>

    public static string JoinNotNullOrEmpty(this string[] objs, string separator)

    {

        var items = new List<string>();

        foreach (string s in objs)

        {

            if (!String.IsNullOrEmpty(s))

                items.Add(s);

        }

        return String.Join(separator, items.ToArray());

    }







    /// <summary>

    /// Calculates the SHA1 hash of the supplied string and returns a base 64 string.

    /// </summary>

    /// <param name="stringToHash">String that must be hashed.</param>

    /// <returns>The hashed string or null if hashing failed.</returns>

    /// <exception cref="ArgumentException">Occurs when stringToHash or key is null or empty.</exception>

    public static string GetSHA1Hash(this string stringToHash)

    {

        if (string.IsNullOrEmpty(stringToHash)) return null;

        //{

        //    throw new ArgumentException("An empty string value cannot be hashed.");

        //}



        Byte[] data = Encoding.UTF8.GetBytes(stringToHash);

        Byte[] hash = new SHA1CryptoServiceProvider().ComputeHash(data);

        return Convert.ToBase64String(hash);

    }



    /// <summary>

    /// Determines whether the string contains any of the provided values.

    /// </summary>

    /// <param name="value"></param>

    /// <param name="values"></param>

    /// <returns></returns>

    public static bool ContainsAny(this string value, params string[] values)

    {

        return value.ContainsAny(StringComparison.CurrentCulture, values);

    }



    /// <summary>

    /// Determines whether the string contains any of the provided values.

    /// </summary>

    /// <param name="value"></param>

    /// <param name="comparisonType"></param>

    /// <param name="values"></param>

    /// <returns></returns>

    public static bool ContainsAny(this string value, StringComparison comparisonType, params string[] values)

    {

        return values.Any(v => value.IndexOf(v, comparisonType) > -1);



    }



    /// <summary>

    /// Determines whether the string contains all of the provided values.

    /// </summary>

    /// <param name="value"></param>

    /// <param name="values"></param>

    /// <returns></returns>

    public static bool ContainsAll(this string value, params string[] values)

    {

        return value.ContainsAll(StringComparison.CurrentCulture, values);

    }



    /// <summary>

    /// Determines whether the string contains all of the provided values.

    /// </summary>

    /// <param name="value"></param>

    /// <param name="comparisonType"></param>

    /// <param name="values"></param>

    /// <returns></returns>

    public static bool ContainsAll(this string value, StringComparison comparisonType, params string[] values)

    {

        return values.All(v => value.IndexOf(v, comparisonType) > -1);

    }



    /// <summary>

    /// Determines whether the string is equal to any of the provided values.

    /// </summary>

    /// <param name="value"></param>

    /// <param name="comparisonType"></param>

    /// <param name="values"></param>

    /// <returns></returns>

    public static bool EqualsAny(this string value, StringComparison comparisonType, params string[] values)

    {

        return values.Any(v => value.Equals(v, comparisonType));

    }



    /// <summary>

    /// Wildcard comparison for any pattern

    /// </summary>

    /// <param name="value">The current <see cref="System.String"/> object</param>

    /// <param name="patterns">The array of string patterns</param>

    /// <returns></returns>

    public static bool IsLikeAny(this string value, params string[] patterns)

    {

        return patterns.Any(p => value.IsLike(p));

    }



    /// <summary>

    /// Wildcard comparison

    /// </summary>

    /// <param name="value"></param>

    /// <param name="pattern"></param>

    /// <returns></returns>

    public static bool IsLike(this string value, string pattern)

    {

        if (value == pattern) return true;



        if (pattern[0] == '*' && pattern.Length > 1)

        {

            for (int index = 0; index < value.Length; index++)

            {

                if (value.Substring(index).IsLike(pattern.Substring(1)))

                    return true;

            }

        }

        else if (pattern[0] == '*')

        {

            return true;

        }

        else if (pattern[0] == value[0])

        {

            return value.Substring(1).IsLike(pattern.Substring(1));

        }

        return false;

    }



    /// <summary>

    /// Truncates a string with optional Elipses added

    /// </summary>

    /// <param name="value"></param>

    /// <param name="length"></param>

    /// <param name="useElipses"></param>

    /// <returns></returns>

    public static string Truncate(this string value, int length, bool useElipses = false)

    {

        int e = useElipses ? 3 : 0;

        if (length - e <= 0) throw new InvalidOperationException(string.Format("Length must be greater than {0}.", e));



        if (string.IsNullOrEmpty(value) || value.Length <= length) return value;



        return value.Substring(0, length - e) + new String('.', e);

    }

}

}

}