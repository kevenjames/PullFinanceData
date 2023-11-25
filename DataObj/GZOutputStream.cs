using PullFinanceData.Util;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using System.Text;

namespace PullFinanceData.DataObj
{
    /// <summary>
    /// The z_stream struct in zlib
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct z_stream
    {
        public IntPtr next_in;      // next input byte 
        public int avail_in;        // number of bytes available at next_in 
        public int total_in;        // total nb of input bytes read so far 

        public IntPtr next_out;     // next output byte should be put there 
        public int avail_out;       // remaining free space at next_out 
        public int total_out;       // total nb of bytes output so far 

        [MarshalAs(UnmanagedType.LPStr)]
        public string msg;          // last error message, NULL if no error 
        public IntPtr state;            // not visible by applications 

        public IntPtr zalloc;           // used to allocate the internal state 
        public IntPtr zfree;            // used to free the internal state 
        public IntPtr opaque;           // private data object passed to zalloc and zfree 

        public int data_type;       // best guess about the data type: ascii or binary 
        public uint adler;          // adler32 value of the uncompressed data 
        public IntPtr reserved;     // reserved for future use 
    }

    public class API
    {
        public static readonly string ZLIB_VERSION = "1.2.3";
        public const string AssemblyName = "zlib1";

        public enum CompressLevel
        {
            Z_DEFAULT_COMPRESSION = -1,
            Z_NO_COMPRESSION = 0,
            Z_BEST_SPEED = 1,
            Z_BEST_COMPRESSION = 9,

            Z_COMPRESSION_0 = 0,
            Z_COMPRESSION_1 = 1,
            Z_COMPRESSION_2 = 2,
            Z_COMPRESSION_3 = 3,
            Z_COMPRESSION_4 = 4,
            Z_COMPRESSION_5 = 5,
            Z_COMPRESSION_6 = 6,
            Z_COMPRESSION_7 = 7,
            Z_COMPRESSION_8 = 8,
            Z_COMPRESSION_9 = 9,
        }

        /// <summary>
        /// The ratio to reserve memory for uncompress. Too low, an ArgumentOutOfRangeException may be thrown
        /// indicating uncompress buffer too small. Too high, we may run out of memory.
        /// </summary>
        public static int InflateReserveRatio
        {
            get
            {
                return inflate_reserve_ratio;
            }
            set
            {
                inflate_reserve_ratio = value;
            }
        }

        public static byte[] Compress(byte[] source)
        {
            return Compress(source, CompressLevel.Z_DEFAULT_COMPRESSION);
        }

        public static byte[] Compress(byte[] source, CompressLevel level)
        {
            int res = Z_OK;

            int sourceLen = source.Length;
            int destLen = sourceLen + sourceLen / 1000 + 12;
            byte[] destination = new byte[destLen];

            unsafe
            {
                IntPtr pDestLen = new IntPtr(&destLen);
                fixed (byte* src = source, dest = destination)
                {
                    IntPtr pSrc = new IntPtr(src);
                    IntPtr pDest = new IntPtr(dest);
                    res = compress2(pDest, pDestLen, pSrc, (uint)sourceLen, (int)level);
                }

            }
            switch (res)
            {
                case Z_OK:
                    return LeadingBytes(destination, destLen);
                case Z_STREAM_ERROR:
                    throw new ArgumentOutOfRangeException("level");
                case Z_MEM_ERROR:
                    throw new OutOfMemoryException();
                case Z_BUF_ERROR:
                    throw new ArgumentOutOfRangeException("destination", "there was not enough room in the output buffer");
                default:
                    throw new Exception("unknown error");
            }
        }

        public static byte[] Uncompress(byte[] source)
        {
            int res = Z_OK;

            int sourceLen = source.Length;
            int destLen = sourceLen * inflate_reserve_ratio;
            byte[] destination = new byte[destLen];

            unsafe
            {
                IntPtr pDestLen = new IntPtr(&destLen);
                fixed (byte* src = source, dest = destination)
                {
                    IntPtr pSrc = new IntPtr(src);
                    IntPtr pDest = new IntPtr(dest);
                    res = uncompress(pDest, pDestLen, pSrc, (uint)sourceLen);
                }

            }
            switch (res)
            {
                case Z_OK:
                    return LeadingBytes(destination, destLen);
                case Z_BUF_ERROR:
                    throw new ArgumentOutOfRangeException("destination", "there was not enough room in the output buffer");
                case Z_DATA_ERROR:
                    throw new ArgumentException("the input data was corrupted", "source");
                case Z_MEM_ERROR:
                    throw new OutOfMemoryException();
                default:
                    throw new Exception("unknown error");
            }
        }

        public static int Compress2(byte[] source, byte[] destination)
        {
            return Compress2(source, 0, source.Length, destination, 0, CompressLevel.Z_DEFAULT_COMPRESSION);
        }

        public static int Compress2(byte[] source, byte[] destination, int destinationStart)
        {
            return Compress2(source, 0, source.Length, destination, destinationStart, CompressLevel.Z_DEFAULT_COMPRESSION);
        }

        public static int Compress2(byte[] source, int sourceStart, int sourceLen,
            byte[] destination, int destinationStart)
        {
            return Compress2(source, sourceStart, sourceLen, destination, destinationStart,
                CompressLevel.Z_DEFAULT_COMPRESSION);
        }

        /// <summary>
        /// compress data into pre-allocated buffer
        /// </summary>
        /// <param name="source">source buffer</param>
        /// <param name="sourceStart">input start position</param>
        /// <param name="sourceLen">input byte-length</param>
        /// <param name="destination">output buffer</param>
        /// <param name="destinationStart">output start position</param>
        /// <param name="level">Compression Level</param>
        /// <returns>destination output byte-length</returns>
        public static int Compress2(byte[] source, int sourceStart, int sourceLen,
            byte[] destination, int destinationStart, CompressLevel level)
        {
            if (sourceStart < 0)
                throw new ArgumentOutOfRangeException("sourceStart");
            if (sourceLen < 0 || source.Length < sourceStart + sourceLen)
                throw new ArgumentOutOfRangeException("sourceLen");
            if (destinationStart < 0)
                throw new ArgumentOutOfRangeException("destinationStart");

            int destLen = destination.Length - destinationStart;
            if (destLen < 13)
                throw new ArgumentOutOfRangeException("dest", "there was not enough room in the output buffer");

            int res = Z_OK;
            unsafe
            {
                IntPtr pDestLen = new IntPtr(&destLen);
                fixed (byte* src = source, dest = destination)
                {
                    IntPtr pSrc = new IntPtr(src + sourceStart);
                    IntPtr pDest = new IntPtr(dest + destinationStart);
                    res = compress2(pDest, pDestLen, pSrc, (uint)sourceLen, (int)level);
                }

            }
            switch (res)
            {
                case Z_OK:
                    return destLen;
                case Z_STREAM_ERROR:
                    throw new ArgumentOutOfRangeException("level");
                case Z_MEM_ERROR:
                    throw new OutOfMemoryException();
                case Z_BUF_ERROR:
                    throw new ArgumentOutOfRangeException("destination", "there was not enough room in the output buffer");
                default:
                    throw new Exception("unknown error");
            }
        }

        /// <summary>
        /// uncompress data into pre-allocated buffer
        /// </summary>
        /// <param name="source">source buffer</param>
        /// <param name="sourceStart">input start position</param>
        /// <param name="sourceLen">input byte-length</param>
        /// <param name="destination">output buffer</param>
        /// <param name="destinationStart">output start position</param>
        /// <returns>destination output byte-length</returns>
        public static int Uncompress2(byte[] source, int sourceStart, int sourceLen,
            byte[] destination, int destinationStart)
        {
            if (sourceStart < 0)
                throw new ArgumentOutOfRangeException("sourceStart");
            if (sourceLen < 0 || source.Length < sourceStart + sourceLen)
                throw new ArgumentOutOfRangeException("sourceLen");
            if (destinationStart < 0)
                throw new ArgumentOutOfRangeException("destinationStart");

            int destLen = destination.Length - destinationStart;
            if (destLen < 0)
                throw new ArgumentOutOfRangeException("dest", "there was not enough room in the output buffer");

            int res = Z_OK;
            unsafe
            {
                IntPtr pDestLen = new IntPtr(&destLen);
                fixed (byte* src = source, dest = destination)
                {
                    IntPtr pSrc = new IntPtr(src + sourceStart);
                    IntPtr pDest = new IntPtr(dest + destinationStart);
                    res = uncompress(pDest, pDestLen, pSrc, (uint)sourceLen);
                }

            }
            switch (res)
            {
                case Z_OK:
                    return destLen;
                case Z_BUF_ERROR:
                    throw new ArgumentOutOfRangeException("destination", "there was not enough room in the output buffer");
                case Z_DATA_ERROR:
                    throw new ArgumentException("the input data was corrupted", "source");
                case Z_MEM_ERROR:
                    throw new OutOfMemoryException();
                default:
                    throw new Exception("unknown error");
            }
        }

        public static uint Adler32(byte[] source)
        {
            uint adler = adler32(0, IntPtr.Zero, 0);
            return Adler32(adler, source);
        }

        public static uint Adler32(uint adler, byte[] source)
        {
            unsafe
            {
                int sourceLen = source.Length;
                fixed (byte* src = source)
                {
                    IntPtr pSrc = new IntPtr(src);
                    return adler32(adler, pSrc, (uint)sourceLen);
                }
            }
        }


        public static uint CRC32(byte[] source)
        {
            uint crc = crc32(0, IntPtr.Zero, 0);
            return CRC32(crc, source);
        }

        public static uint CRC32(uint crc, byte[] source)
        {
            unsafe
            {
                int sourceLen = source.Length;
                fixed (byte* src = source)
                {
                    IntPtr pSrc = new IntPtr(src);
                    return crc32(crc, pSrc, (uint)sourceLen);
                }
            }
        }

        /// <summary>
        /// For "deflate" compression
        /// </summary>
        internal static int deflateInit(ref z_stream pStrm, int level)
        {
            return deflateInit_(ref pStrm, level, ZLIB_VERSION, Marshal.SizeOf(pStrm));
        }

        /// <summary>
        /// For advanced compression including gzip
        /// </summary>
        internal static int deflateInit2(ref z_stream pStrm, int level,
            int method, int windowBits, int memLevel, int strategy)
        {
            return deflateInit2_(ref pStrm, level, method, windowBits, memLevel, strategy,
                ZLIB_VERSION, Marshal.SizeOf(pStrm));
        }

        internal static int inflateInit(ref z_stream pStrm)
        {
            return inflateInit_(ref pStrm, ZLIB_VERSION, Marshal.SizeOf(pStrm));
        }

        internal static int inflateInit2(ref z_stream pStrm, int windowBits)
        {
            return inflateInit2_(ref pStrm, windowBits, ZLIB_VERSION, Marshal.SizeOf(pStrm));
        }

        /// <summary>
        /// Convert ASCII string to bytes
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] StringToBytes(string s)
        {
            int len = s.Length;
            byte[] bytes = new byte[len];
            for (int i = 0; i < len; i++)
            {
                bytes[i] = (byte)s[i];
            }
            return bytes;
        }

        /// <summary>
        /// Convert bytes to ASCII string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string BytesToString(byte[] data)
        {
            int len = data.Length;
            StringBuilder sb = new StringBuilder(len);
            for (int i = 0; i < len; i++)
            {
                sb.Append((char)data[i]);
            }
            return sb.ToString(0, len);
        }

        /// <summary>
        /// Get the leading bytes of length len within data.
        /// </summary>
        /// <param name="data">The original data buffer</param>
        /// <param name="len">The desired length, cannot be greater than data.Length</param>
        /// <returns>data of desired length</returns>
        private static byte[] LeadingBytes(byte[] data, int len)
        {
            if (data.Length > len)
            {
                byte[] res = new byte[len];
                Buffer.BlockCopy(data, 0, res, 0, len);
                return res;
            }
            else
                return data;
        }


        private API() { }

        // zlib api return values
        internal const int Z_OK = 0;
        internal const int Z_STREAM_END = 1;
        internal const int Z_NEED_DICT = 2;
        internal const int Z_ERRNO = -1;
        internal const int Z_STREAM_ERROR = -2;
        internal const int Z_DATA_ERROR = -3;
        internal const int Z_MEM_ERROR = -4;
        internal const int Z_BUF_ERROR = -5;
        internal const int Z_VERSION_ERROR = -6;

        // other zlib const used
        internal const int Z_DEFLATED = 8;
        internal const int MAX_WBITS = 15;  // 32K LZ77 window
        internal const int Z_DEFAULT_STRATEGY = 0;
        internal const int Z_DEFAULT_MEMLEVEL = 8;  // From 1 to 9

        internal const int Z_UNKNOWN = 2;


        internal const int Z_BUFSIZE = 4096;
        internal const int Z_NO_FLUSH = 0;
        internal const int Z_FINISH = 4;

        // Our estimation of compression ratio
        private static int inflate_reserve_ratio = 10;


        // TODO: There's an unhandled exception when calling this in debugging.
        // Even though return value appeared to be fine.
        [DllImport(AssemblyName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        internal static extern string zlibVersion();

        /// <summary>
        /// The wrap function for zlib!compress2()
        /// </summary>
        [DllImport(AssemblyName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int compress2(IntPtr dest, IntPtr destLen, IntPtr source, UInt32 sourceLen, Int32 level);

        /// <summary>
        /// The wrap function for zlib!uncompress()
        /// </summary>
        [DllImport(AssemblyName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uncompress(IntPtr dest, IntPtr destLen, IntPtr source, UInt32 sourceLen);

        /// <summary>
        /// The wrap function for zlib!adler32()
        /// </summary>
        [DllImport(AssemblyName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint adler32(UInt32 adler, IntPtr buf, UInt32 len);

        /// <summary>
        /// The wrap function for zlib!adler32()
        /// </summary>
        [DllImport(AssemblyName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint crc32(UInt32 crc, IntPtr buf, UInt32 len);

        /// <summary>
        /// For "deflate" compression
        /// </summary>
        /// <param name="pStrm"></param>
        /// <param name="level"></param>
        /// <param name="version"></param>
        /// <param name="stream_size"></param>
        /// <returns></returns>
        [DllImport(AssemblyName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int deflateInit_(ref z_stream pStrm,
            int level, [In, MarshalAs(UnmanagedType.LPStr)] string version, int stream_size);

        /// <summary>
        /// For advanced compression including gzip
        /// </summary>
        /// <param name="pStrm"></param>
        /// <param name="level"></param>
        /// <param name="version"></param>
        /// <param name="stream_size"></param>
        /// <returns></returns>
        [DllImport(AssemblyName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int deflateInit2_(ref z_stream pStrm,
            int level, int method, int windowBits, int memLevel, int strategy,
            [In, MarshalAs(UnmanagedType.LPStr)] string version, int stream_size);

        [DllImport(AssemblyName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int deflate(ref z_stream pStrm,
            int flush);

        [DllImport(AssemblyName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int deflateEnd(ref z_stream pStrm);

        [DllImport(AssemblyName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int inflateInit_(ref z_stream pStrm,
            [In, MarshalAs(UnmanagedType.LPStr)] string version, int stream_size);

        [DllImport(AssemblyName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int inflateInit2_(ref z_stream pStrm, int windowBits,
            [In, MarshalAs(UnmanagedType.LPStr)] string version, int stream_size);

        [DllImport(AssemblyName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int inflate(ref z_stream pStrm, int flush);

        [DllImport(AssemblyName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int inflateEnd(ref z_stream pStrm);
    }

    /// <summary>
    /// Gzip output stream using zlib
    /// </summary>
    public class GZOutputStream : System.IO.Stream, IDisposable
    {
        private static readonly byte[] magic0 = { 0x1F, 0x8B, 8, 4 };
        private static readonly byte[] magic1 = { 0x1F, 0x8B, 8, 12 };
        private static readonly byte[] DW_ZERO = { 0, 0, 0, 0 };

        public GZOutputStream(System.IO.Stream stream) : this(stream, false)
        {
        }

        public GZOutputStream(System.IO.Stream stream, bool bLeaveOpen)
        {
            m_bLeaveOpen = bLeaveOpen;
            m_stream = stream;
            m_stream.Write(magic0, 0, 4); // signature (2), cm, flg (FEXTRA)
            m_stream.Write(DW_ZERO, 0, 4); // mtime (4)
            m_stream.Write(DW_ZERO, 0, 4); // xfl, os, xlen (2)

            InitZStream();
            int iRes = API.deflateInit2(ref m_zstrm, (int)API.CompressLevel.Z_DEFAULT_COMPRESSION,
                API.Z_DEFLATED, -API.MAX_WBITS, API.Z_DEFAULT_MEMLEVEL, API.Z_DEFAULT_STRATEGY);
            Debug.Assert(iRes == API.Z_OK);
        }

        /// <summary>
        /// This will create our "pak" standard gzip stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uri"></param>
        public GZOutputStream(System.IO.Stream stream, string uri) : this(stream, uri, 0, false)
        {
        }

        /// <summary>
        /// This will create our "pak" standard gzip stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uri"></param>
        /// <param name="mtime"></param>
        /// <param name="bLeaveOpen"></param>
        public GZOutputStream(System.IO.Stream stream, string uri, uint mtime, bool bLeaveOpen)
        {
            m_bLeaveOpen = bLeaveOpen;
            m_stream = stream;

            m_stream.Write(magic1, 0, 4); // signature (2), cm, flg (FEXTRA|FNAME)
            StreamUtil.WriteMSB(m_stream, mtime); // mtime
            m_stream.Write(DW_ZERO, 0, 4); // xfl, os, xlen (2)
            byte[] filename = System.Text.Encoding.UTF8.GetBytes(uri);
            m_stream.Write(filename, 0, filename.Length);
            m_stream.WriteByte(0);

            InitZStream();
            int iRes = API.deflateInit2(ref m_zstrm, (int)API.CompressLevel.Z_DEFAULT_COMPRESSION,
                API.Z_DEFLATED, -API.MAX_WBITS, API.Z_DEFAULT_MEMLEVEL, API.Z_DEFAULT_STRATEGY);
            Debug.Assert(iRes == API.Z_OK);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback,
            object state)
        {
            throw new NotSupportedException();
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback,
            object state)
        {
            // TODO:  Add GZipOutputStream.BeginWrite implementation
            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            // We are closed already!
            // Debug.Assert(m_deflateBuffer != IntPtr.Zero) ;

            if (m_deflateBuffer != IntPtr.Zero)
            {
                int iRes;
                while ((iRes = API.deflate(ref m_zstrm, API.Z_FINISH)) == API.Z_OK)
                {
                    WriteBufferAndReset();
                }

                Debug.Assert(iRes == API.Z_STREAM_END);

                if (API.Z_STREAM_END == iRes)
                {
                    WriteBufferAndReset();
                }

                iRes = API.deflateEnd(ref m_zstrm);
                Debug.Assert(iRes == API.Z_OK);

                Marshal.FreeHGlobal(m_deflateBuffer);
                m_deflateBuffer = IntPtr.Zero;

                StreamUtil.WriteMSB(m_stream, m_crc32);
                StreamUtil.WriteMSB(m_stream, m_isize);
                if (m_bLeaveOpen)
                {
                    m_stream.Flush();
                }
                else
                {
                    m_stream.Close();
                }

                base.Close();
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            // TODO:  Add GZipOutputStream.EndWrite implementation
            base.EndWrite(asyncResult);
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override int ReadByte()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // Note: This can be inefficient, bufferred writing is needed
            // for performance if applicable.
            if (count > 0)
            {
                //unsafe
                //{
                //    fixed (byte* bytes = buffer)
                //    {
                //        IntPtr data = new IntPtr(bytes + offset);
                //        m_zstrm.avail_in = m_zstrm.total_in = count;
                //        m_zstrm.next_in = data;
                //        m_crc32 = API.crc32(m_crc32, data, (uint)count);
                //        while (m_zstrm.avail_in > 0)
                //        {
                //            if (m_zstrm.avail_out == 0)
                //            {
                //                WriteBufferAndReset();
                //            }

                //            int iRes = API.deflate(ref m_zstrm, API.Z_NO_FLUSH);
                //            if (iRes == API.Z_OK)
                //            {
                //                continue;
                //            }
                //            else
                //            {
                //                throw new Exception();
                //            }
                //        }

                //        if (m_zstrm.avail_out == 0)
                //        {
                //            WriteBufferAndReset();
                //        }

                //        m_isize += (uint)count;
                //    }
                //}
            }
        }

        public override void WriteByte(byte value)
        {
            // Note: This can be inefficient, bufferred writing is needed
            // for performance if applicable.
            byte[] data = new byte[1];
            data[0] = value;
            Write(data, 0, 1);
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        private bool m_bDisposed;

        public new void Dispose()
        {
            // dispose of the managed and unmanaged resources
            Dispose(true);

            // tell the GC that the Finalize process no longer needs
            // to be run for this object.
            GC.SuppressFinalize(this);
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            // process only if mananged and unmanaged resources have
            // not been m_bDisposed of.
            if (!this.m_bDisposed)
            {
                if (disposeManagedResources)
                {
                    // dispose managed resources
                    Close();
                }

                // dispose unmanaged resources
                m_bDisposed = true;
            }
        }

        private bool m_bLeaveOpen;
        private System.IO.Stream m_stream;

        private uint m_crc32;
        private uint m_isize;
        private int m_bufSize;
        private IntPtr m_deflateBuffer;

        private z_stream m_zstrm;

        private void InitZStream()
        {
            m_bufSize = API.Z_BUFSIZE;
            m_deflateBuffer = Marshal.AllocHGlobal(m_bufSize);

            m_zstrm.next_in = IntPtr.Zero;
            m_zstrm.avail_in = 0;
            m_zstrm.total_in = 0;

            m_zstrm.next_out = m_deflateBuffer;
            m_zstrm.avail_out = m_bufSize;
            m_zstrm.total_out = m_bufSize;

            m_zstrm.msg = null;
            m_zstrm.state = IntPtr.Zero;

            m_zstrm.zalloc = IntPtr.Zero;
            m_zstrm.zfree = IntPtr.Zero;
            m_zstrm.opaque = IntPtr.Zero;

            m_zstrm.data_type = API.Z_UNKNOWN;
            m_zstrm.adler = 0;
            m_zstrm.reserved = IntPtr.Zero;

            m_crc32 = API.crc32(0, IntPtr.Zero, 0);
            m_isize = 0;
        }

        private void WriteBufferAndReset()
        {
            // TODO: Is there anyway we don't have to do this copying for efficiency?
            int count = m_bufSize - m_zstrm.avail_out;
            byte[] bytes = new byte[count];
            Marshal.Copy(m_deflateBuffer, bytes, 0, count);
            m_stream.Write(bytes, 0, count);

            m_zstrm.avail_out = m_bufSize;
            m_zstrm.next_out = m_deflateBuffer;
        }
    }
}