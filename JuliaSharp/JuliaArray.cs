using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using size_t = System.UIntPtr;


namespace JuliaSharp
{
    public class JuliaArray<T> : SafeHandle
    {
        private static Dictionary<string, IntPtr> TypePair = new Dictionary<string, IntPtr>()
        {
            { "Double", (IntPtr)JuliaNative.Float64Type },
            { "Int64",  (IntPtr)JuliaNative.Int64Type }
        };

        private JuliaArray() : base(IntPtr.Zero, true)
        {
            handle = IntPtr.Zero;
        }

        private jl_array_t struc;

        public JuliaArray(IntPtr handle) : base(handle, true)
        {
            SetHandle(handle);
            struc = Marshal.PtrToStructure<jl_array_t>(handle);
        }

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

        public static JuliaArray<T> Create1D(int n)
        {
            var type = typeof(T);

            if (!TypePair.ContainsKey(type.Name))
            {
                throw new ArgumentException("仅支持基本数据类型");
            }



            //IntPtr arrayType = JuliaNative.jl_apply_array_type(jlFloat64, 1);
            #region get an exported extern variable
            IntPtr juliaDLL = Win32.LoadLibraryA("libjulia.dll");
            // Debug.Assert(juliaDLL != IntPtr.Zero);
            
            //`pJlFloat64` is a pointer to the pointer variable `jl_float64_type`
            IntPtr pJlFloat64 = Win32.GetProcAddress(juliaDLL, "jl_float64_type");
            // dereference `pJlFloat64` to obtain the pointer variable
            // Use `PtrToStructure` in general, but better to use the dedicated `ReadIntPtr`
            //IntPtr jlFloat64 = Marshal.PtrToStructure<IntPtr>(pJlFloat64);
            IntPtr jlFloat64 = Marshal.ReadIntPtr(pJlFloat64);
            //Debug.Assert(jlFloat64 != IntPtr.Zero);

            Win32.FreeLibrary(juliaDLL);
            #endregion


            var arrartype = JuliaNative.ApplyArrayType(TypePair[type.Name], (size_t)1);
            //double vv = JuliaNative.UnboxFloat64(arrartype);
            try
            {


                IntPtr arrayType = JuliaNative.jl_apply_array_type(jlFloat64, (UInt64)1);
                IntPtr jlVector = JuliaNative.jl_alloc_array_1d(arrayType, (UInt64)n);
                //var x = JuliaNative.AllocArray1D(arrartype, (size_t)n);
                return new JuliaArray<T>(jlVector);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
            
        }

        public static JuliaArray<T> Create2D(int nr, int nc)
        {
            var type = typeof(T);

            if (!TypePair.ContainsKey(type.Name))
            {
                throw new ArgumentException("仅支持基本数据类型");
            }

            var arrartype = JuliaNative.ApplyArrayType(TypePair[type.Name], (size_t)2);
            var x = JuliaNative.AllocArray2D(arrartype, (size_t)nr, (size_t)nc);
            return new JuliaArray<T>(x);
        }

        public unsafe Span<T> GetSpan()
        {
            return new Span<T>(struc.data.ToPointer(), (int)struc.length);
        }

        public int Ndims => struc.flags.ndims;
        public int Nrows => (int)struc.nrows;
        public int Ncols => (int)struc.ncols;
        public int Length => (int)struc.length;

        public static implicit operator JuliaArray<T>(IntPtr ptr)
        {
            return new JuliaArray<T>(ptr);
        }

        public static implicit operator IntPtr(JuliaArray<T> value)
        {
            return value.GetHandle();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            var rs = GetSpan();
            var ncols = Ncols;

            if (Ndims == 1)
            {
                sb.Append($"{Length}-element ");
                ncols = 1;
            }
            else
            {
                sb.Append($"{Nrows}×{Ncols} ");
            }

            sb.AppendLine($"Array{{{typeof(T).Name},{Ndims}}}:");

            for (int i = 0; i < Nrows; i++)
            {
                for (int j = 0; j < ncols; j++)
                {
                    sb.Append(" "+rs[i * ncols + j]);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct jl_array_t
    {
        public IntPtr data;
        public size_t length;
        public jl_array_flags_t flags;
        public UInt16 elsize;
        public UInt32 offset;
        public size_t nrows;
        //public size_t maxsize;// 1d
        public size_t ncols;// Nd
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct jl_array_flags_t
    {
        public UInt16 aggr_byte;

        public UInt16 how  => (UInt16)(aggr_byte & 0x3);
        public UInt16 ndims => (UInt16)((aggr_byte >> 2) & 0x3FF);
        public UInt16 pooled => (UInt16)((aggr_byte >> 12) & 0x1);
        public UInt16 ptarray => (UInt16)((aggr_byte >> 13) & 0x1);
        public UInt16 isshared => (UInt16)((aggr_byte >> 14) & 0x1);
        public UInt16 isaligned => (UInt16)((aggr_byte >> 15) & 0x1);
    }
}
