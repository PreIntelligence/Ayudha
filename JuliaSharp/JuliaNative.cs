using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using size_t = System.UIntPtr;

namespace JuliaSharp
{
    public class JuliaNative
    {
        private const string LibraryName = "libjulia";

        const int RTLD_NOW = 0x002;
        const int RTLD_GLOBAL = 0x100;
        [DllImport(@"libdl")]
        static extern IntPtr dlopen(string filename, int flags);
        [DllImport("libdl")]
        protected static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string filename);

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);

        private static IntPtr LibPtr = IntPtr.Zero;

        static JuliaNative()
        {
            Debug.WriteLine("OSArchitecture:{0}", RuntimeInformation.OSArchitecture);

            try
            {
                var arch = @"x64";
                if (RuntimeInformation.OSArchitecture == Architecture.X64)
                {
                    //仅支持64位
                    arch = @"x64";
                }
                else
                {
                    Debug.WriteLine("仅支持64位");
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var libName = Path.Combine(@"C:\Julia\bin\", $"{ LibraryName}.dll");
                    Debug.WriteLine($"windows:{libName}");
                    LibPtr = LoadLibrary(libName);

                    InitThreading = Jl_init__threading;
                    AtExitHook = Jl_atexit_hook;
                    EvalString = Jl_eval_string;
                    UnboxFloat64 = Jl_unbox_float64;
                    BoxFloat64 = Jl_box_float64;
                    UnboxInt64 = Jl_unbox_int64;

                    

                    UnboxString = Jl_unbox_string;

                    BoxInt64 = Jl_box_int64;
                    GetGlobal = Jl_get_global;
                    Call1 = Jl_call1;
                    Symbol = Jl_symbol;
                    ApplyArrayType = Jl_apply_array_type;
                    AllocArray1D = Jl_alloc_array_1d;
                    AllocArray2D = Jl_alloc_array_2d;

                    Float64Type = GetProcAddress(LibPtr, "jl_float64_type");
                    Int64Type = GetProcAddress(LibPtr, "jl_int64_type");
                    BaseModule = GetProcAddress(LibPtr, "jl_base_module");
                    MainModule = GetProcAddress(LibPtr, "jl_main_module");


                    value1 = Jl_value_t;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    var libName = Path.Combine(AppContext.BaseDirectory, "julia", $"linux-{arch}", $"{LibraryName}.so");
                    Debug.WriteLine($"linux:{libName}");
                    LibPtr = dlopen(libName, RTLD_NOW | RTLD_GLOBAL);


                    if (LibPtr != IntPtr.Zero)
                    {
                        var ptr1 = dlsym(LibPtr, "jl_init");
                        InitThreading = Marshal.GetDelegateForFunctionPointer<JuliaInitDelegate>(ptr1);

                        ptr1 = dlsym(LibPtr, "jl_atexit_hook");
                        AtExitHook = Marshal.GetDelegateForFunctionPointer<JuliaAtExitHookDelegate>(ptr1);

                        ptr1 = dlsym(LibPtr, "jl_eval_string");
                        EvalString = Marshal.GetDelegateForFunctionPointer<JuliaEvalStringDelegate>(ptr1);

                        ptr1 = dlsym(LibPtr, "jl_unbox_float64");
                        UnboxFloat64 = Marshal.GetDelegateForFunctionPointer<JuliaUnboxFloat64Delegate>(ptr1);


                       


                        ptr1 = dlsym(LibPtr, "jl_box_float64");
                        BoxFloat64 = Marshal.GetDelegateForFunctionPointer<JuliaBoxFloat64Delegate>(ptr1);

                        ptr1 = dlsym(LibPtr, "jl_get_global");
                        GetGlobal = Marshal.GetDelegateForFunctionPointer<JuliaGetGlobalDelegate>(ptr1);

                        ptr1 = dlsym(LibPtr, "jl_unbox_int64");
                        UnboxInt64 = Marshal.GetDelegateForFunctionPointer<JuliaUnboxInt64Delegate>(ptr1);

                        ptr1 = dlsym(LibPtr, "jl_box_int64");
                        BoxInt64 = Marshal.GetDelegateForFunctionPointer<JuliaBoxInt64Delegate>(ptr1);

                        ptr1 = dlsym(LibPtr, "jl_call1");
                        Call1 = Marshal.GetDelegateForFunctionPointer<JuliaCall1Delegate>(ptr1);

                        ptr1 = dlsym(LibPtr, "jl_symbol");
                        Symbol = Marshal.GetDelegateForFunctionPointer<JuliaSymbolDelegate>(ptr1);

                        ptr1 = dlsym(LibPtr, "jl_apply_array_type");
                        ApplyArrayType = Marshal.GetDelegateForFunctionPointer<JuliaApplyArrayTypeDelegate>(ptr1);

                        ptr1 = dlsym(LibPtr, "jl_alloc_array_1d");
                        AllocArray1D = Marshal.GetDelegateForFunctionPointer<JuliaAllocArray1DDelegate>(ptr1);

                        ptr1 = dlsym(LibPtr, "jl_alloc_array_2d");
                        AllocArray2D = Marshal.GetDelegateForFunctionPointer<JuliaAllocArray2DDelegate>(ptr1);

                        Float64Type = dlsym(LibPtr, "jl_float64_type");
                        Int64Type = dlsym(LibPtr, "jl_int64_type");
                        BaseModule = dlsym(LibPtr, "jl_base_module");
                    }
                }
                else
                {
                    Debug.WriteLine("仅支持Windows和Linux");
                }
                if (LibPtr != IntPtr.Zero)
                    Debug.WriteLine("加载Julia成功!");
                else
                {
                    throw new BadImageFormatException("加载Julia失败");
                }
            }
            catch (Exception ex)
            {
                throw new BadImageFormatException(ex.Message);
            }
        }



        public delegate void JuliaInitDelegate();
        [DllImport(LibraryName, EntryPoint = "jl_init__threading", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Jl_init__threading();
        public static JuliaInitDelegate InitThreading = null;

        public delegate void JuliaAtExitHookDelegate(int status);
        [DllImport(LibraryName, EntryPoint = "jl_atexit_hook", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Jl_atexit_hook(int status);
        public static JuliaAtExitHookDelegate AtExitHook = null;

        public delegate IntPtr JuliaEvalStringDelegate([MarshalAs(UnmanagedType.LPStr)] string str);
        [DllImport(LibraryName, EntryPoint = "jl_eval_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Jl_eval_string([MarshalAs(UnmanagedType.LPStr)] string str);
        public static JuliaEvalStringDelegate EvalString = null;

        public delegate double JuliaUnboxFloat64Delegate(IntPtr v);
        [DllImport(LibraryName, EntryPoint = "jl_unbox_float64", CallingConvention = CallingConvention.Cdecl)]
        private static extern double Jl_unbox_float64(IntPtr v);
        public static JuliaUnboxFloat64Delegate UnboxFloat64 = null;



        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern object jl_get_function(object m, string name);

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern object jl_call0(object m);





    


        //
        public delegate IntPtr JuliaBoxFloat64Delegate(double x);
        [DllImport(LibraryName, EntryPoint = "jl_box_float64", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Jl_box_float64(double x);
        public static JuliaBoxFloat64Delegate BoxFloat64 = null;

        public delegate long JuliaUnboxInt64Delegate(IntPtr v);
        [DllImport(LibraryName, EntryPoint = "jl_unbox_int64", CallingConvention = CallingConvention.Cdecl)]
        private static extern long Jl_unbox_int64(IntPtr v);
        public static JuliaUnboxInt64Delegate UnboxInt64 = null;





        // string
        public delegate String JuliaUnboxStringDelegate(IntPtr v);
        [DllImport(LibraryName, EntryPoint = "jl_unbox_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern String Jl_unbox_string(IntPtr v);
        public static JuliaUnboxStringDelegate UnboxString = null;



        //
        public delegate long JuliaValueDelegate(IntPtr v);
        [DllImport(LibraryName, EntryPoint = "jl_value_t", CallingConvention = CallingConvention.Cdecl)]
        private static extern long Jl_value_t(IntPtr v);
        public static JuliaValueDelegate value1 = null;
        //



        public delegate IntPtr JuliaBoxInt64Delegate(long x);
        [DllImport(LibraryName, EntryPoint = "jl_box_int64", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Jl_box_int64(long x);
        public static JuliaBoxInt64Delegate BoxInt64 = null;

        public delegate IntPtr JuliaGetGlobalDelegate(IntPtr m, IntPtr var);
        [DllImport(LibraryName, EntryPoint = "jl_get_global", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Jl_get_global(IntPtr m, IntPtr var);
        public static JuliaGetGlobalDelegate GetGlobal = null;

        public delegate IntPtr JuliaSymbolDelegate([MarshalAs(UnmanagedType.LPStr)] string str);
        [DllImport(LibraryName, EntryPoint = "jl_symbol", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Jl_symbol([MarshalAs(UnmanagedType.LPStr)] string str);
        public static JuliaSymbolDelegate Symbol = null;

        //[DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern object jl_get_function(object m, string name);


        public delegate IntPtr JuliaGetFunctionDelegate(IntPtr m, string name);
       // [DllImport(LibraryName, EntryPoint = "jl_getfunction", CallingConvention = CallingConvention.Cdecl)]
        public static JuliaGetFunctionDelegate GetFunction = new JuliaGetFunctionDelegate((m, name) =>
        {
            var bstr = Symbol(name);
            return GetGlobal(m , bstr);
        });

        public delegate IntPtr JuliaCall1Delegate(IntPtr f, IntPtr a);
        [DllImport(LibraryName, EntryPoint = "jl_call1", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Jl_call1(IntPtr f, IntPtr a);
        public static JuliaCall1Delegate Call1 = null;

        public delegate IntPtr JuliaApplyArrayTypeDelegate(IntPtr type, size_t dim);
        [DllImport(LibraryName, EntryPoint = "jl_apply_array_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Jl_apply_array_type(IntPtr type, size_t dim);
        public static JuliaApplyArrayTypeDelegate ApplyArrayType = null;

        public delegate IntPtr JuliaAllocArray1DDelegate(IntPtr atype, size_t nr);
        [DllImport(LibraryName, EntryPoint = "jl_alloc_array_1d", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Jl_alloc_array_1d(IntPtr atype, size_t nr);
        public static JuliaAllocArray1DDelegate AllocArray1D = null;

        public delegate IntPtr JuliaAllocArray2DDelegate(IntPtr atype, size_t nr, size_t nc);
        [DllImport(LibraryName, EntryPoint = "jl_alloc_array_2d", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Jl_alloc_array_2d(IntPtr atype, size_t nr, size_t nc);
        public static JuliaAllocArray2DDelegate AllocArray2D = null;





        //new from shauhah

        // const char *jl_typeof_str(jl_value_t *v)
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr jl_typeof_str(IntPtr v);


        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr jl_typename_str(IntPtr value);

        // jl_value_t *jl_apply_array_type(jl_value_t *type, size_t dim)
        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr jl_apply_array_type(IntPtr type, UInt64 dim);

        // jl_array_t *jl_alloc_array_1d(jl_value_t *atype, size_t nr);
        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr jl_alloc_array_1d(IntPtr type, UInt64 n);






        public static IntPtr Float64Type;
        public static IntPtr Int64Type;
        public static IntPtr BaseModule;
        public static IntPtr MainModule;
        struct JuliaModuleT
        {
            //JL_DATA_TYPE
            IntPtr name;
            IntPtr parent;
            HTableT bindings;
            ArrayListT usings;
            byte istopmod;
            UInt64 uuid;
            size_t primary_world;
            UInt32 counter;
        }

        const int HT_N_INLINE = 32;

        struct HTableT
        {
            size_t size;
            IntPtr table;//void **table
            IntPtr _space;//void *_space[HT_N_INLINE]
        }

        struct ArrayListT
        {
            size_t len;
            size_t max;
            IntPtr items;//void **items;
            IntPtr _space;//void *_space[AL_N_INLINE];
        }

        struct JlSymT
        {
            IntPtr left;
            IntPtr right;
            UIntPtr hash;
        }

        //typedef struct _jl_module_t
        //{
        //    JL_DATA_TYPE
        //        jl_sym_t* name;
        //    struct _jl_module_t *parent;
        //        htable_t bindings;
        //    arraylist_t usings;  // modules with all bindings potentially imported
        //    uint8_t istopmod;
        //    uint64_t uuid;
        //    size_t primary_world;
        //    uint32_t counter;
        //}
        //jl_module_t;


    }
}
