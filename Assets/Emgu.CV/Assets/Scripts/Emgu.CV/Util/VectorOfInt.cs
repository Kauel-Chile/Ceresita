﻿//----------------------------------------------------------------------------
//
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
//
//  Vector of Int
//
//  This file is automatically generated, do not modify.
//----------------------------------------------------------------------------



using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Emgu.CV.Structure;
#if !(NETFX_CORE || NETCOREAPP1_1 || NETSTANDARD1_4)
using System.Runtime.Serialization;
#endif

namespace Emgu.CV.Util
{
   /// <summary>
   /// Wrapped class of the C++ standard vector of Int.
   /// </summary>
#if !(NETFX_CORE || NETCOREAPP1_1 || NETSTANDARD1_4)
   [Serializable]
   [DebuggerTypeProxy(typeof(VectorOfInt.DebuggerProxy))]
#endif
   public partial class VectorOfInt : Emgu.Util.UnmanagedObject, IInputOutputArray
#if !(NETFX_CORE || NETCOREAPP1_1 || NETSTANDARD1_4)
   , ISerializable
#endif
   {
      private readonly bool _needDispose;
   
      static VectorOfInt()
      {
         CvInvoke.CheckLibraryLoaded();
         Debug.Assert(Emgu.Util.Toolbox.SizeOf<int>() == SizeOfItemInBytes, "Size do not match");
      }

#if !(NETFX_CORE || NETCOREAPP1_1 || NETSTANDARD1_4)
      /// <summary>
      /// Constructor used to deserialize runtime serialized object
      /// </summary>
      /// <param name="info">The serialization info</param>
      /// <param name="context">The streaming context</param>
      public VectorOfInt(SerializationInfo info, StreamingContext context)
         : this()
      {
         Push((int[])info.GetValue("IntArray", typeof(int[])));
      }
	  
      /// <summary>
      /// A function used for runtime serialization of the object
      /// </summary>
      /// <param name="info">Serialization info</param>
      /// <param name="context">Streaming context</param>
      public void GetObjectData(SerializationInfo info, StreamingContext context)
      {
         info.AddValue("IntArray", ToArray());
      }
#endif

      /// <summary>
      /// Create an empty standard vector of Int
      /// </summary>
      public VectorOfInt()
         : this(VectorOfIntCreate(), true)
      {
      }
	  
      internal VectorOfInt(IntPtr ptr, bool needDispose)
      {
         _ptr = ptr;
         _needDispose = needDispose;
      }

      /// <summary>
      /// Create an standard vector of Int of the specific size
      /// </summary>
      /// <param name="size">The size of the vector</param>
      public VectorOfInt(int size)
         : this( VectorOfIntCreateSize(size), true)
      {
      }
	  
      /// <summary>
      /// Create an standard vector of Int with the initial values
      /// </summary>
      /// <param name="values">The initial values</param>
      public VectorOfInt(int[] values)
         :this()
      {
         Push(values);
      }
	  
      /// <summary>
      /// Push an array of value into the standard vector
      /// </summary>
      /// <param name="value">The value to be pushed to the vector</param>
      public void Push(int[] value)
      {
         if (value.Length > 0)
         {
            GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            VectorOfIntPushMulti(_ptr, handle.AddrOfPinnedObject(), value.Length);
            handle.Free();
         }
      }
      
      /// <summary>
      /// Push multiple values from the other vector into this vector
      /// </summary>
      /// <param name="other">The other vector, from which the values will be pushed to the current vector</param>
      public void Push(VectorOfInt other)
      {
         VectorOfIntPushVector(_ptr, other);
      }
	  
      /// <summary>
      /// Convert the standard vector to an array of Int
      /// </summary>
      /// <returns>An array of Int</returns>
      public int[] ToArray()
      {
         int[] res = new int[Size];
         if (res.Length > 0)
         {
            GCHandle handle = GCHandle.Alloc(res, GCHandleType.Pinned);
            VectorOfIntCopyData(_ptr, handle.AddrOfPinnedObject());
            handle.Free();
         }
         return res;
      }

      /// <summary>
      /// Get the size of the vector
      /// </summary>
      public int Size
      {
         get
         {
            return VectorOfIntGetSize(_ptr);
         }
      }

      /// <summary>
      /// Clear the vector
      /// </summary>
      public void Clear()
      {
         VectorOfIntClear(_ptr);
      }

      /// <summary>
      /// The pointer to the first element on the vector. In case of an empty vector, IntPtr.Zero will be returned.
      /// </summary>
      public IntPtr StartAddress
      {
         get
         {
            return VectorOfIntGetStartAddress(_ptr);
         }
      }
	  
      /// <summary>
      /// Get the item in the specific index
      /// </summary>
      /// <param name="index">The index</param>
      /// <returns>The item in the specific index</returns>
      public int this[int index]
      {
         get
         {
            int result = new int();
            VectorOfIntGetItem(_ptr, index, ref result);
            return result;
         }
      }

      /// <summary>
      /// Release the standard vector
      /// </summary>
      protected override void DisposeObject()
      {
         if (_needDispose && _ptr != IntPtr.Zero)
            VectorOfIntRelease(ref _ptr);
      }

      /// <summary>
      /// Get the pointer to cv::_InputArray
      /// </summary>
      /// <returns>The input array </returns>
      public InputArray GetInputArray()
      {
         return new InputArray( cvInputArrayFromVectorOfInt(_ptr), this );
      }
	  
      /// <summary>
      /// Get the pointer to cv::_OutputArray
      /// </summary>
      /// <returns>The output array </returns>
      public OutputArray GetOutputArray()
      {
         return new OutputArray( cvOutputArrayFromVectorOfInt(_ptr), this );
      }

      /// <summary>
      /// Get the pointer to cv::_InputOutputArray
      /// </summary>
      /// <returns>The input output array </returns>
      public InputOutputArray GetInputOutputArray()
      {
         return new InputOutputArray( cvInputOutputArrayFromVectorOfInt(_ptr), this );
      }
      
      /// <summary>
      /// The size of the item in this Vector, counted as size in bytes.
      /// </summary>
      public static int SizeOfItemInBytes
      {
         get { return VectorOfIntSizeOfItemInBytes(); }
      }
	  
      internal class DebuggerProxy
      {
         private VectorOfInt _v;

         public DebuggerProxy(VectorOfInt v)
         {
            _v = v;
         }

         public int[] Values
         {
            get { return _v.ToArray(); }
         }
      }

      [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
      internal static extern IntPtr VectorOfIntCreate();

      [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
      internal static extern IntPtr VectorOfIntCreateSize(int size);

      [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
      internal static extern void VectorOfIntRelease(ref IntPtr v);

      [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
      internal static extern int VectorOfIntGetSize(IntPtr v);

      [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
      internal static extern void VectorOfIntCopyData(IntPtr v, IntPtr data);

      [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
      internal static extern IntPtr VectorOfIntGetStartAddress(IntPtr v);

      [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
      internal static extern void VectorOfIntPushMulti(IntPtr v, IntPtr values, int count);

      [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
      internal static extern void VectorOfIntPushVector(IntPtr ptr, IntPtr otherPtr);
      
      [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
      internal static extern void VectorOfIntClear(IntPtr v);

      [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
      internal static extern void VectorOfIntGetItem(IntPtr vec, int index, ref int element);

      [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
      internal static extern int VectorOfIntSizeOfItemInBytes();
      
      [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
      internal static extern IntPtr cvInputArrayFromVectorOfInt(IntPtr vec);

      [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
      internal static extern IntPtr cvOutputArrayFromVectorOfInt(IntPtr vec);

      [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
      internal static extern IntPtr cvInputOutputArrayFromVectorOfInt(IntPtr vec);
   }
}


