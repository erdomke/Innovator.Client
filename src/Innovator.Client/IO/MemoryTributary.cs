using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;

namespace Innovator.Client
{
  /// <summary>
  /// MemoryTributary is a re-implementation of MemoryStream that uses a dynamic list of byte arrays as a backing store, instead of a single byte array, the allocation
  /// of which will fail for relatively small streams as it requires contiguous memory.
  /// </summary>
  /// <remarks>Adapted from <a href="https://www.codeproject.com/Articles/348590/A-replacement-for-MemoryStream">Code Project: A replacement for MemoryStream</a></remarks>
  public class MemoryTributary : Stream, IXmlStream
  {
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryTributary"/> class.
    /// </summary>
    public MemoryTributary()
    {
      Position = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryTributary"/> class.
    /// </summary>
    /// <param name="source">The source data.</param>
    public MemoryTributary(byte[] source)
    {
      this.Write(source, 0, source.Length);
      Position = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryTributary"/> class.
    /// </summary>
    /// <param name="length">The length.</param>
    /// <remarks><paramref name="length"/> is largely ignored because capacity has no meaning unless we implement an artifical limit</remarks>
    public MemoryTributary(int length)
    {
      SetLength(length);
      Position = length;
      byte[] d = Block;   //access block to prompt the allocation of memory
      Position = 0;
    }

    #endregion

    #region Status Properties

    /// <summary>Gets a value indicating whether the current stream supports reading.</summary>
    /// <returns>Always <c>true</c> as the stream supports reading.</returns>
    public override bool CanRead
    {
      get { return true; }
    }

    /// <summary>Gets a value indicating whether the current stream supports seeking.</summary>
    /// <returns>Always <c>true</c> as the stream supports seeking.</returns>
    public override bool CanSeek
    {
      get { return true; }
    }

    /// <summary>Gets a value indicating whether the current stream supports writing.</summary>
    /// <returns>Always <c>true</c> as the stream supports writing.</returns>
    public override bool CanWrite
    {
      get { return true; }
    }

    #endregion

    #region Public Properties

    /// <summary>Gets the length in bytes of the stream.</summary>
    /// <returns>A long value representing the length of the stream in bytes.</returns>
    /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
    public override long Length
    {
      get
      {
        if (_blocks == null)
          throw new ObjectDisposedException("MemoryTributary");
        return _length;
      }
    }

    /// <summary>Gets or sets the position within the current stream.</summary>
    /// <returns>The current position within the stream.</returns>
    /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
    public override long Position
    {
      get
      {
        if (_blocks == null)
          throw new ObjectDisposedException("MemoryTributary");
        return _position;
      }
      set
      {
        if (_blocks == null)
          throw new ObjectDisposedException("MemoryTributary");
        _position = value;
      }
    }

    #endregion

    #region Members

    private long _length = 0;
    private long _blockSize = 65536;
    private List<byte[]> _blocks = new List<byte[]>();
    private long _position;

    #endregion

    #region Internal Properties

    /* Use these properties to gain access to the appropriate block of memory for the current Position */

    /// <summary>
    /// The block of memory currently addressed by Position
    /// </summary>
    protected byte[] Block
    {
      get
      {
        while (_blocks.Count <= BlockId)
          _blocks.Add(new byte[_blockSize]);
        return _blocks[(int)BlockId];
      }
    }

    /// <summary>
    /// The id of the block currently addressed by Position
    /// </summary>
    protected long BlockId
    {
      get { return Position / _blockSize; }
    }

    /// <summary>
    /// The offset of the byte currently addressed by Position, into the block that contains it
    /// </summary>
    protected long BlockOffset
    {
      get { return Position % _blockSize; }
    }

    #endregion

    #region Public Stream Methods

    /// <summary>Does nothing</summary>
    public override void Flush()
    {
    }

    /// <summary>Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.</summary>
    /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
    /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source. </param>
    /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream. </param>
    /// <param name="count">The maximum number of bytes to be read from the current stream. </param>
    /// <exception cref="ArgumentNullException"><paramref name="buffer" /> is null. </exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset" /> or <paramref name="count" /> is negative. </exception>
    /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
    public override int Read(byte[] buffer, int offset, int count)
    {
      var lcount = (long)count;

      if (lcount < 0)
        throw new ArgumentOutOfRangeException("count", lcount, "Number of bytes to copy cannot be negative.");
      
      var remaining = (_length - Position);
      if (lcount > remaining)
        lcount = remaining;

      if (_blocks == null)
        throw new ObjectDisposedException("MemoryTributary");
      if (buffer == null)
        throw new ArgumentNullException("buffer", "Buffer cannot be null.");
      if (offset < 0)
        throw new ArgumentOutOfRangeException("offset", offset, "Destination offset cannot be negative.");
      
      int read = 0;
      long copysize = 0;
      do
      {
        copysize = Math.Min(lcount, _blockSize - BlockOffset);
        Buffer.BlockCopy(Block, (int)BlockOffset, buffer, offset, (int)copysize);
        lcount -= copysize;
        offset += (int)copysize;

        read += (int)copysize;
        Position += copysize;

      } while (lcount > 0);

      return read;

    }

    /// <summary>Sets the position within the current stream.</summary>
    /// <returns>The new position within the current stream.</returns>
    /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter. </param>
    /// <param name="origin">A value of type <see cref="SeekOrigin" /> indicating the reference point used to obtain the new position. </param>
    /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
    public override long Seek(long offset, SeekOrigin origin)
    {
      if (_blocks == null)
        throw new ObjectDisposedException("MemoryTributary");

      switch (origin)
      {
        case SeekOrigin.Begin:
          Position = offset;
          break;
        case SeekOrigin.Current:
          Position += offset;
          break;
        case SeekOrigin.End:
          Position = Length - offset;
          break;
      }
      return Position;
    }

    /// <summary>Sets the length of the current stream.</summary>
    /// <param name="value">The desired length of the current stream in bytes. </param>
    /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
    /// <remarks><paramref name="value"/> is largely ignored because capacity has no meaning unless we implement an artifical limit</remarks>
    public override void SetLength(long value)
    {
      if (_blocks == null)
        throw new ObjectDisposedException("MemoryTributary");

      _length = value;
    }

    /// <summary>Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.</summary>
    /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream. </param>
    /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream. </param>
    /// <param name="count">The number of bytes to be written to the current stream. </param>
    /// <exception cref="ObjectDisposedException"><see cref="Write(byte[], int, int)" /> was called after the stream was closed.</exception>
    public override void Write(byte[] buffer, int offset, int count)
    {
      if (_blocks == null)
        throw new ObjectDisposedException("MemoryTributary");

      long initialPosition = Position;
      int copysize;
      try
      {
        do
        {
          copysize = Math.Min(count, (int)(_blockSize - BlockOffset));

          EnsureCapacity(Position + copysize);

          Buffer.BlockCopy(buffer, (int)offset, Block, (int)BlockOffset, copysize);
          count -= copysize;
          offset += copysize;

          Position += copysize;

        } while (count > 0);
      }
      catch (Exception)
      {
        Position = initialPosition;
        throw;
      }
    }

    /// <summary>Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.</summary>
    /// <returns>The unsigned byte cast to an <see cref="Int32"/>, or -1 if at the end of the stream.</returns>
    /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
    public override int ReadByte()
    {
      if (_blocks == null)
        throw new ObjectDisposedException("MemoryTributary");

      if (Position >= _length)
        return -1;

      byte b = Block[BlockOffset];
      Position++;

      return b;
    }

    /// <summary>Writes a byte to the current position in the stream and advances the position within the stream by one byte.</summary>
    /// <param name="value">The byte to write to the stream. </param>
    /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
    public override void WriteByte(byte value)
    {
      if (_blocks == null)
        throw new ObjectDisposedException("MemoryTributary");

      EnsureCapacity(Position + 1);
      Block[BlockOffset] = value;
      Position++;
    }

    /// <summary>
    /// Guarantees that the underlying memory has at least the specified number of bytes of capacity
    /// </summary>
    /// <param name="intendedLength">Intended length of the data</param>
    protected void EnsureCapacity(long intendedLength)
    {
      if (intendedLength > _length)
        _length = (intendedLength);
    }

    #endregion

    #region IDispose

    /// <summary>Releases the unmanaged resources used by the <see cref="Stream" /> and optionally releases the managed resources.</summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
      _blocks = null;
      base.Dispose(disposing);
    }

    #endregion

    #region Public Additional Helper Methods

    /// <summary>
    /// Returns the entire content of the stream as a byte array. This is not safe because the call to new byte[] may 
    /// fail if the stream is large enough. Where possible use methods which operate on streams directly instead.
    /// </summary>
    /// <returns>A byte[] containing the current data in the stream</returns>
    public byte[] ToArray()
    {
      long firstposition = Position;
      Position = 0;
      byte[] destination = new byte[Length];
      Read(destination, 0, (int)Length);
      Position = firstposition;
      return destination;
    }

    /// <summary>
    /// Reads length bytes from source into the this instance at the current position.
    /// </summary>
    /// <param name="source">The stream containing the data to copy</param>
    /// <param name="length">The number of bytes to copy</param>
    public void ReadFrom(Stream source, long length)
    {
      byte[] buffer = new byte[4096];
      int read;
      do
      {
        read = source.Read(buffer, 0, (int)Math.Min(4096, length));
        length -= read;
        this.Write(buffer, 0, read);

      } while (length > 0);
    }

    /// <summary>
    /// Writes the entire stream into destination, regardless of Position, which remains unchanged.
    /// </summary>
    /// <param name="destination">The stream to write the content of this stream to</param>
    public void WriteTo(Stream destination)
    {
      long initialpos = Position;
      Position = 0;
      this.CopyTo(destination);
      Position = initialpos;
    }

    /// <summary>
    /// Creates an <see cref="XmlReader" /> for reading XML from the stream
    /// </summary>
    /// <returns>An <see cref="XmlReader" /> for reading XML</returns>
    public XmlReader CreateReader()
    {
      return XmlReader.Create(this);
    }

    #endregion
  }
}
