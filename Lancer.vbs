Dim shell, fso, scriptDir
Set shell = CreateObject("WScript.Shell")
Set fso   = CreateObject("Scripting.FileSystemObject")
scriptDir = fso.GetParentFolderName(WScript.ScriptFullName)

' Lance l'application sans fenetre CMD visible (style 0 = cache)
shell.Run "cmd /c """ & scriptDir & "\Lancer.bat""", 0, False
