#include <Windows.h>

// !!! THIS THING ISNT ACTIVE AT THE MOMENT !!!
// it may be used in the future but who knows..

bool Hook(void * funcToHook, void * funcToExecute, int lenght) {
	if (lenght < 5)
		return false;

	DWORD currentProtection;
	VirtualProtect(funcToHook, lenght, PAGE_EXECUTE_READWRITE, &currentProtection);

	memset(funcToExecute, 0x90, lenght);

	DWORD relativeAdress = ((DWORD)funcToExecute - (DWORD)funcToHook) - 5;

	*(BYTE*)funcToHook = 0xE9;
	*(DWORD*)((DWORD)funcToHook + 1) = relativeAdress;

	DWORD temp;
	VirtualProtect(funcToHook, lenght, currentProtection, &temp);

	return true;
}

DWORD jumpBackAddress;
void __declspec(naked) FunctionToExecute() {
	__asm {
		jmp [jumpBackAddress]
	}
}

DWORD WINAPI MainThread(LPVOID param) {
	while (true) {
		if (GetAsyncKeyState(VK_NUMPAD0)) break;
		Sleep(50);
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