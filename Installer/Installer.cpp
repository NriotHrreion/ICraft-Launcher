#include <iostream>
#include "Installer.h";

using namespace std;

void start_install()
{
	cout << "[Installer] Start Install" << endl;

	system("cd game & npm install");
}
