using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace MainApp
{
	public partial class MainClient
	{

		//Microsoft.Win32.SafeHandles
		//SafeHandles API 참조

		// public const short FILE_ATTRIBUTE_NORMAL = 0x80;
		// public const short INVALID_HANDLE_VALUE = -1;
		public const uint GENERIC_READ = 0x80000000;
		// public const uint GENERIC_WRITE = 0x40000000;
		// public const uint CREATE_NEW = 1;
		// public const uint CREATE_ALWAYS = 2;
		public const uint OPEN_EXISTING = 3;

		private SafeFileHandle handleValue = null;

		// Win32 API 참조
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool GetFileSizeEx(SafeFileHandle hFile, out long lpFileSize);

		/// <summary>
		/// 파일의 길이(용량)을 얻는 함수
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		private long GetFileLength(string filePath)
		{
			try
			{
				// 핸들 작성
				handleValue = CreateFile(filePath, GENERIC_READ, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

				if (!handleValue.IsInvalid)
				{
					if (GetFileSizeEx(handleValue, out long fileSize))
					{
						return fileSize;
					}
					else
					{
						throw new Exception("파일을 열 수 없습니다");
					}
				}
				else
				{
					throw new Exception("파일을 열 수 없습니다");
				}
			}
			catch(Exception)
			{
				throw;
			}
			finally
			{
				handleValue.Dispose();
			}
			
		}

	}
}
