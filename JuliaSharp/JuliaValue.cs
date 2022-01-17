using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace JuliaSharp
{
    public class JuliaValue<T> : SafeHandle where T : IConvertible
    {
        private JuliaValue(): base(IntPtr.Zero,true)
        {
            handle = IntPtr.Zero;
        }

        public JuliaValue(IntPtr handle) : base(handle, true) { SetHandle(handle); }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            return true;
        }

        public IntPtr GetHandle()
        {
            if (IsInvalid)
            {
                throw new Exception("The handle is invalid.");
            }
            else
            {
                bool bSuccess = false;
                DangerousAddRef(ref bSuccess);
                return handle;
            }
        }

        public void ReturnHandle(IntPtr h)
        {
            if ((h == handle) && (!IsInvalid))
            {
                DangerousRelease();
            }
        }

        public long GetInt64()
        {
            return JuliaNative.UnboxInt64(handle);
        }

        public double GetFloat64()
        {
            return JuliaNative.UnboxFloat64(handle);
        }

        //public String GetString()
        //{
        //    return JuliaNative.UnboxString(handle);
        //}

        public T Get()
        {
            switch (typeof(T).Name)
            {
                case "Double":
                    return (T)Convert.ChangeType(GetFloat64(), typeof(T));
                case "Int64":
                    return (T)Convert.ChangeType(GetInt64(), typeof(T));
                //case "String":
                 //   return (T)Convert.ChangeType(GetString(), typeof(T));
                default:
                    throw new ArgumentException("仅支持基本数据类型");
            }
        }
       
        private static JuliaValue<T> Box(long value)
        {
            var p = JuliaNative.BoxInt64(value);
            return new JuliaValue<T>(p);
        }

        private static JuliaValue<T> Box(double value)
        {
            var p = JuliaNative.BoxFloat64(value);
            return new JuliaValue<T>(p);
        }

        public static JuliaValue<T> Create(T value)
        {
            switch (value)
            {
                case double d:
                    return Box(d);
                case long d:
                    return Box(d);
                default:
                    throw new ArgumentException("仅支持基本数据类型");
            }
        }

        public static implicit operator JuliaValue<T>(IntPtr ptr)
        {
            return new JuliaValue<T>(ptr);
        }

        public static implicit operator IntPtr(JuliaValue<T> value)
        {
            return value.GetHandle();
        }

        public static implicit operator T(JuliaValue<T> value)
        {
            return value.Get();
        }

        public static implicit operator JuliaValue<T>(T value)
        {
            return Create(value);
        }
    }
}
