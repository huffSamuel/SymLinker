﻿using SymLinker.Linker.LinkCreators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace SymLinker.Linker
{
    public class Linker
    {
        #region Actions
        // Attach to these to get warnings, errors, and informaion out of SymLinkerCore
        public Action<string> OnWarn;
        public Action<string> OnError;
        public Action<string> OnInfo;
        #endregion

        private string source;
        private string dest;
        private string destFileName;
        private List<Error> errors = new List<Error>();
        private ISymLinkCreator linker = null;

        /// <summary>
        /// Creates a new Symbolic Link Creator
        /// </summary>
        public Linker()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                linker = new WindowsSymLinkCreator();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                linker = new LinuxSymLinkCreator();
            }
            else
            {
                linker = new OSXSymLinkCreator();
            }
        }

        /// <summary>
        /// Creates a symbolic link from <paramref name="source"/> to <paramref name="dest"/> 
        /// </summary>
        /// <param name="source">Source file</param>
        /// <param name="dest">Destination directory</param>
        /// <returns>
        /// Returns true if the system was able to create the SymLink
        /// </returns>
        public bool CreateLink(string source, string dest)
        {
            this.source = source;
            this.dest = dest;

            bool success = false;
            if (!CheckLinkReadiness())
            {
                WriteError(ResolveError());
            }
            else
            {
                if (CopyFile())
                {
                    CreateLink();
                }
            }

            return success;
        }

        /// <summary>
        /// Creates a symbolic link
        /// </summary>
        private void CreateLink()
        {
            DeleteFile(source);

            try
            {
                var sourcePath = Path.GetDirectoryName(source);

                var linkMade = linker.CreateSymLink(sourcePath, destFileName, true);

                if (linkMade)
                {
                    DeleteFile(source);
                }
                else
                {
                    DeleteFile(destFileName);
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Deletes <paramref name="file"/>
        /// </summary>
        /// <returns></returns>
        private bool DeleteFile(string file)
        {
            bool success = false;
            try
            {
                File.Delete(source);
                success = true;
            }
            catch (Exception ex)
            {
                WriteError(ex);
            }
            return success;
        }

        /// <summary>
        /// Performs the copy for the SymLink creation
        /// </summary>
        /// <param name="source">Source file to copy</param>
        /// <param name="dest">Destination path to copy the file to</param>
        /// <returns></returns>
        private bool CopyFile()
        {
            bool success = false;
            var fileName = string.Empty;
            try
            {
                fileName = new FileInfo(source).Name;
                destFileName = Path.Combine(dest, fileName);

                File.Copy(source, destFileName, false);
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
            }
            if (!success)
            {
                DeleteFile(destFileName);
                errors.Add(Error.FailedFileCopy);
            }
            return success;
        }

        /// <summary>
        /// Attempts to rollback the file copy 
        /// </summary>
        private void RollbackCopy()
        {
            var fileName = new FileInfo(source).Name;
            var destFileName = Path.Combine(dest, fileName);
            try
            {
                if (File.Exists(destFileName))
                {
                    File.Delete(destFileName);
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Checks for readiness of a drive to perform a SymLink creation. Expects absolute path
        /// </summary>
        /// <param name="source">Source file path</param>
        /// <param name="dest">Destination directory path</param>
        /// <returns>
        /// Returns true if the system is ready to perform a SymLink
        /// </returns>
        private bool CheckLinkReadiness()
        {
            try
            {
                var destDrive = dest.Substring(0, 1)[0];
                var fileSize = 0L;
                var freeSpace = 0L;

                // Check format
                if (!char.IsLetter(destDrive))
                    errors.Add(Error.DestinationNotAbsolutePath);

                // Check existance
                if (File.Exists(source))
                {
                    fileSize = new FileInfo(source).Length;
                    if (fileSize < 1024) WriteWarn("File less than standard block size");
                }
                else
                {
                    errors.Add(Error.FileNotFound);
                    fileSize = -1;
                }

                if (Directory.Exists(dest))
                {
                    freeSpace = new DriveInfo(destDrive.ToString()).AvailableFreeSpace;
                }
                else
                {
                    errors.Add(Error.DirectoryNotFound);
                    freeSpace = 0;
                }

                // Check space
                if (freeSpace <= fileSize)
                {
                    errors.Add(Error.FreeSpaceUnavailable);
                }
            }
            catch (Exception ex)
            {
                errors.Add(Error.Exception);
            }

            return (errors.Count == 0);
        }

        /// <summary>
        /// Resolves all errors encountered to their readable strings
        /// </summary>
        /// <returns>
        /// Concatenation of all errors encountered
        /// </returns>
        private string ResolveError()
        {
            var errorBuilder = new StringBuilder();
            var errorCount = errors.Count;
            errorBuilder.Append(errorCount);
            errorBuilder.Append(" error/s found: ");

            for (int i = 0; i < errors.Count; ++i)
            {
                var error = errors[i];
                errorBuilder.Append(ResolveError(error));
                errorBuilder.Append(". ");
            }

            return errorBuilder.ToString();
        }

        /// <summary>
        /// Resolves an individual error to its readable string
        /// </summary>
        /// <param name="errorCode">Error code as found in <see cref="Error"/></param>
        /// <returns>
        /// The readable string for <paramref name="errorCode"/>
        /// </returns>
        private string ResolveError(Error errorCode)
        {
            string errorString = string.Empty;
            switch (errorCode)
            {
                case Error.FileNotFound:
                    errorString = "File not found";
                    break;
                case Error.DirectoryNotFound:
                    errorString = "Directory not found";
                    break;
                case Error.FreeSpaceUnavailable:
                    errorString = "Destination does not have sufficient free space";
                    break;
                case Error.DestinationNotAbsolutePath:
                    errorString = "Destination is not a parsable absolute file path";
                    break;
                default:
                    errorString = "Unknown error occurred";
                    break;
            }

            return errorString;
        }

        #region Message Handlers
        /// <summary>
        /// Writes a string error message to OnError
        /// </summary>
        /// <param name="msg"></param>
        private void WriteError(string msg)
        {
            OnError?.Invoke(msg);
        }

        /// <summary>
        /// Writes exception details to OnError
        /// </summary>
        /// <param name="ex">Exception to write out</param>
        private void WriteError(Exception ex)
        {
            var errorMsg = new StringBuilder(ex.Message);
            var exception = ex.InnerException;
            while (exception != null)
            {
                errorMsg.Append("\nInner Exception: ");
                errorMsg.Append(exception.Message);
                exception = exception.InnerException;
            }
            if (ex.Source != null)
                errorMsg.Append(ex.Source.ToString());
            if (ex.StackTrace != null)
                errorMsg.Append(ex.StackTrace);

            WriteError(errorMsg.ToString());
        }

        /// <summary>
        /// Writes warning message to OnWarn
        /// </summary>
        /// <param name="msg">Message to write</param>
        private void WriteWarn(string msg)
        {
            OnWarn?.Invoke(msg);
        }

        /// <summary>
        /// Writes informational message to OnInfo
        /// </summary>
        /// <param name="msg"></param>
        private void WriteInfo(string msg)
        {
            OnInfo?.Invoke(msg);
        }
        #endregion

        /// <summary>
        /// Enumerated errors this software can generate
        /// </summary>
        private enum Error
        {
            /// <summary>
            /// Source file was not found
            /// </summary>
            FileNotFound = 0,

            /// <summary>
            /// Destination directory was not found
            /// </summary>
            DirectoryNotFound = 1,

            /// <summary>
            /// Destination drive does not have sufficient free space
            /// </summary>
            FreeSpaceUnavailable = 2,

            /// <summary>
            /// Destination path was not parsable as an absolute path
            /// </summary>
            DestinationNotAbsolutePath = 3,

            /// <summary>
            /// Source file exists at the destination already
            /// </summary>
            FileExists = 4,

            /// <summary>
            /// Some exception was encountered
            /// </summary>
            Exception = 5,

            /// <summary>
            /// Failed to copy source to destination
            /// </summary>
            FailedFileCopy,
        }
    }
}
