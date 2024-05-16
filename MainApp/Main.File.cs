using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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

		// Win32 API 참조(Fileapi.h)
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool GetFileSizeEx(SafeFileHandle hFile, out long lpFileSize);

		/// <summary>
		/// https://learn.microsoft.com/ko-kr/windows/win32/api/sysinfoapi/ns-sysinfoapi-memorystatusex
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct MEMORYSTATUSEX
		{
			public uint dwLength;
			public uint dwMemoryLoad;
			public ulong ullTotalPhys;
			public ulong ullAvailPhys;
			public ulong ullTotalPageFile;
			public ulong ullAvailPageFile;
			public ulong ullTotalVirtual;
			public ulong ullAvailVirtual;
			public ulong ullAvailExtendedVirtual;
		}

		// Win32 API 참조(Sysinfoapi.h)
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool GetPhysicallyInstalledSystemMemory(out long ToTotalMemoryInKilobytes);

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
			catch (Exception)
			{
				throw;
			}
			finally
			{
				handleValue.Dispose();
			}

		}

		/// <summary>
		/// 현재 시스템의 메모리를 체크하는 함수
		/// </summary>
		/// <param name="fileSize"></param>
		/// <returns></returns>
		private bool CheckSystemMemory(long fileSize, out string errorMsg)
		{
			errorMsg = string.Empty;

			if (GetPhysicallyInstalledSystemMemory(out long memorySize))
			{
				// 파일 용량이 시스템 전체 메모리의 30% 이상 인경우 에러 출력
				if (fileSize >= (memorySize * 1024) * 0.3)
				{
					errorMsg = "시스템의 메모리가 부족합니다. 파일을 처리 할 수 없습니다.";
					return false;
				}

				// 구조체 객체생성
				MEMORYSTATUSEX memoryStatus = new MEMORYSTATUSEX
				{
					dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX))
				};

				// 사용 가능한 메모리 확인
				if (GlobalMemoryStatusEx(ref memoryStatus))
				{
					if (fileSize > (long)memoryStatus.ullAvailPhys)
					{
						if (MessageBox.Show("시스템에서 사용 가능한 메모리가 부족합니다.\n파일 처리를 계속 진행시 프로그램이 멈추거나 에러가 발생 할 수 있습니다.\n그래도 진행하시겠습니까?", " 경고", MessageBoxButtons.YesNo) == DialogResult.No)
						{
							errorMsg = "작업을 취소했습니다.";
							return false;
						}
						else
						{
							return true;
						}
					}
					else
					{
						return true;
					}
				}
				else
				{
					return true;
				}
			}
			else
			{
				return true;
			}
		}
	}
}
