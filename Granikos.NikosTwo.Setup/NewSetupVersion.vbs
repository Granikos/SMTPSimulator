''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
''  Increment the version number of an MSI setup project
''  and update relevant GUIDs
''  
''  Hans-Jürgen Schmidt / 19.12.2007  
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
set a = wscript.arguments
if a.count = 0 then wscript.quit 1

'read and backup project file
Set fso = CreateObject("Scripting.FileSystemObject")
Set f = fso.OpenTextFile(a(0))
s = f.ReadAll
f.Close
fbak = a(0) & ".bak"
if fso.fileexists(fbak) then fso.deletefile fbak
fso.movefile a(0), fbak

'find, increment and replace version number
set re = new regexp
re.global = true
re.pattern = "(""ProductVersion"" = ""8:)(\d+(\.\d+)+)"""
set m = re.execute(s)
v = m(0).submatches(1)
v1 = split(v, ".")
v1(ubound(v1)) = v1(ubound(v1)) + 1
vnew = join(v1, ".")
'msgbox v & " --> " & vnew
s = re.replace(s, "$1" & vnew & """")

'replace ProductCode
re.pattern = "(""ProductCode"" = ""8:)(\{.+\})"""
guid = CreateObject("Scriptlet.TypeLib").Guid
guid = left(guid, len(guid) - 2)
s = re.replace(s, "$1" & guid & """")

'replace PackageCode
re.pattern = "(""PackageCode"" = ""8:)(\{.+\})"""
guid = CreateObject("Scriptlet.TypeLib").Guid
guid = left(guid, len(guid) - 2)
s = re.replace(s, "$1" & guid & """")

'write project file
fnew = a(0)
set f = fso.CreateTextfile(fnew, true)
f.write(s)
f.close