using System;
using System.IO;

namespace PersonApp
{
    public class FileResourceManager : IDisposable
    {
        private FileStream _fileStream;
        private StreamWriter _writer;
        private StreamReader _reader;
        private bool _disposed = false;
        private readonly string _filePath;

        public FileResourceManager(string filePath, FileMode mode)
        {
            _filePath = filePath;
            
            try
            {
                _fileStream = new FileStream(filePath, mode, FileAccess.ReadWrite, FileShare.Read);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to open file: {filePath}", ex);
            }
        }

        public void OpenForWriting()
        {
            CheckDisposed();
            
            if (_writer != null)
                _writer.Dispose();
            
            _writer = new StreamWriter(_fileStream, System.Text.Encoding.UTF8);
            _writer.AutoFlush = true;
        }

        public void OpenForReading()
        {
            CheckDisposed();
            
            if (_reader != null)
                _reader.Dispose();

            _fileStream.Position = 0;
            _reader = new StreamReader(_fileStream, System.Text.Encoding.UTF8);
        }

        public void WriteLine(string text)
        {
            CheckDisposed();
            
            if (_writer == null)
                OpenForWriting();
            
            _writer.WriteLine(text);
        }
        public string ReadAllText()
        {
            CheckDisposed();
            
            if (_reader == null)
                OpenForReading();
            
            return _reader.ReadToEnd();
        }

        public void AppendText(string text)
        {
            CheckDisposed();

            _fileStream.Position = _fileStream.Length;
            
            if (_writer == null)
                OpenForWriting();
            
            _writer.Write(text);
        }

        public string GetFileInfo()
        {
            CheckDisposed();
            
            if (!File.Exists(_filePath))
                return "File does not exist";
            
            FileInfo fileInfo = new FileInfo(_filePath);
            return $"File: {fileInfo.Name}\n" +
                   $"Size: {fileInfo.Length} bytes\n" +
                   $"Created: {fileInfo.CreationTime}\n" +
                   $"Modified: {fileInfo.LastWriteTime}";
        }

        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(FileResourceManager));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _writer?.Dispose();
                    _reader?.Dispose();
                    _fileStream?.Dispose();
                }
                
                _disposed = true;
            }
        }
        
        ~FileResourceManager()
        {
            Dispose(false);
        }
    }
}