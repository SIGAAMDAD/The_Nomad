using System.Diagnostics;
using System.IO;

public class AntiCheat {
	string[] BadPrograms = {
		"cheatengine.exe"
	};

	public void CheckForProcesses() {
		Process[] processList = Process.GetProcesses();
		for ( int i = 0; i < processList.Length; i++ ) {
			for ( int p = 0; p < BadPrograms.Length; p++ ) {
				if ( Path.GetFileName( processList[i].MainModule.FileName ) == BadPrograms[p] ) {
					processList[i].Kill();
				}
			}
		}
	}
};