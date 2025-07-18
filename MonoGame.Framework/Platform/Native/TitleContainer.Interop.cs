using System;
using System.IO;
using MonoGame.Interop;
using System.Runtime.InteropServices;

internal unsafe class ReadOnlyAssetStream : Stream
{
    private MG_Asset* _asset;
    private long _length;
    private long _position;

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => _length;

    public override long Position
    {
        get => _position;
        set
        {
            if (value < 0 || value > _length)
                throw new ArgumentOutOfRangeException();

            _position = value;
        }
    }

    public ReadOnlyAssetStream(string assetname)
    {
        if (MG.AssetOpen(assetname, out _asset, out _length) == 0)
            throw new FileNotFoundException("Asset not found", assetname);
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_position + count > _length)
            count = (int)(_length - _position);

        if (count == 0)
            return 0;

        int bytesRead;
        fixed (byte* b = buffer)
            bytesRead = MG.AssetRead(_asset, b, count);
        _position += bytesRead;

        return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        var newPosition = MG.AssetSeek(_asset, offset, (int)origin);
        if (newPosition < 0 || newPosition > _length)
            throw new ArgumentOutOfRangeException();

        _position = newPosition;

        return _position;
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    protected unsafe override void Dispose(bool disposing)
    {
        if (_asset != null)
        {
            MG.AssetClose(_asset);
            _asset = null;
        }

        base.Dispose(disposing);
    }
}

[MGHandle]
internal readonly struct MG_Asset
{
}

internal static unsafe partial class MG
{
    public const string MonoGameNativeDLL = "monogame.native";

    [DllImport(MonoGameNativeDLL, EntryPoint = "MG_Asset_Open", ExactSpelling = true)]
    public static extern byte AssetOpen(string assetname, out MG_Asset* file, out long length);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MG_Asset_Read", ExactSpelling = true)]
    public static extern int AssetRead(MG_Asset* file, byte* buffer, int count);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MG_Asset_Seek", ExactSpelling = true)]
    public static extern long AssetSeek(MG_Asset* file, long offset, int origin);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MG_Asset_Close", ExactSpelling = true)]
    public static extern void AssetClose(MG_Asset* file);

    public static Stream OpenRead(string path)
    {
        return new ReadOnlyAssetStream(path);
    }
}
