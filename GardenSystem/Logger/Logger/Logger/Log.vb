Imports System.IO
Public Class Log

    Private LogName As String
    Private lockobj As New Object

    Sub New(pLogName As String)
        LogName = pLogName
    End Sub

    Private Function DirectoryName() As String
        Dim assembly As System.Reflection.Assembly = Reflection.Assembly.GetExecutingAssembly()
        Dim logPath As String = Path.Combine(Path.GetDirectoryName(assembly.Location), String.Format("Log\{0:0000}\{1:00}\{2:00}\", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))
        Return logPath
    End Function

    Private Function FileName() As String
        Dim logFile As String = Path.Combine(DirectoryName, String.Format("{0}.txt", LogName))
        Return logFile
    End Function

    Public Sub Add(message As String)
        SyncLock lockobj
            message = message.Replace(vbCrLf, " ")
            Dim log As String = String.Format(DateTime.Now.ToString("yyy/MM/dd HH:mm:ss"))

            If Not Directory.Exists(DirectoryName) Then
                Directory.CreateDirectory(DirectoryName)
            End If

            File.AppendAllText(FileName, log & " " & message & Environment.NewLine)
        End SyncLock
    End Sub

    Public Function Read() As String
        Dim wText As String = String.Empty

        SyncLock lockobj
            If File.Exists(FileName) Then
                Try
                    wText = File.ReadAllText(FileName)
                Catch ex As Exception
                    Throw New Exception("Read - " & ex.Message)
                End Try
            End If
        End SyncLock

        Return wText
    End Function

    Public Function ReadLatest(rows As Int16) As String
        Dim wText As String = String.Empty
        Dim r_splittext() As String

        SyncLock lockobj
            If File.Exists(FileName) And Not (rows = 0) Then
                Try
                    r_splittext = File.ReadAllLines(FileName)
                    If rows >= r_splittext.Length Then
                        rows = r_splittext.Length
                    End If
                    For i As Int16 = r_splittext.Length - 1 To r_splittext.Length - rows Step -1
                        wText += (r_splittext(i) + vbCrLf)
                    Next
                Catch ex As Exception
                    Throw New Exception("ReadLatest - " & ex.Message)
                End Try
            End If
        End SyncLock

        Return wText
    End Function
End Class
