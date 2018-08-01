#include <Windows.h>

// !!! THIS THING ISNT ACTIVE AT THE MOMENT !!!
// it may be used in the future but who knows..

typedef void(__stdcall * XAmeisenHook)(char * string);
XAmeisenHook AmeisenHookDoString;

DWORD WINAPI MainThread(LPVOID param) {
	uintptr_t modBase = (uintptr_t)GetModuleHandle(NULL);
	AmeisenHookDoString = (XAmeisenHook)(modBase + 0x819210);

	while (true) 
		if (GetAsyncKeyState(VK_NUMPAD0)) {
			AmeisenHookDoString((char *)"/target player");
		}
	FreeLibraryAndExitThread((HMODULE)param, 0);
	return 0;
}

BOOL WINAPI DllMain(HINSTANCE hModule, DWORD dwReason, LPVOID lpReserved)
{
	switch (dwReason) {
	case DLL_PROCESS_ATTACH:
		CreateThread(0, 0, MainThread, hModule, 0, 0);
		break;

	default:
		break;
	}
	return TRUE;
}