Imports System.IO
Imports System.Security.Cryptography

Public Class Form1

    ' CRCAA
    ' A Zero Collision CRC
    ' 24-03-2026

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim ofd As OpenFileDialog = New OpenFileDialog
        ofd.Filter = "Tutti i files|*.*"

        If ofd.ShowDialog(Me) = DialogResult.OK Then
            Dim filetoCRC As String = ofd.FileName

            If filetoCRC = "" OrElse filetoCRC = Nothing OrElse FileLen(filetoCRC) = 0 Then
                MsgBox("Scegli un file valido", MsgBoxStyle.Exclamation, "FileHarbour")
            End If

            Dim crcAA = DTCMaker(filetoCRC)
            MsgBox("CRCAA = " & crcAA)

            MsgBox("SHA256 =  " & DTCReader(crcAA).Item1)
            MsgBox("DataOra =  " & DTCReader(crcAA).Item2)
        End If
    End Sub

    '---------------------   CRCAA Terapeutico Zero Collisioni   ---------------------------
    'CRCAA  = db7abba433f9f7aa789a21a7072a7d9e6bb2859d5de61db88008075f4b98959dbbafcacdbabfff
    'CRCAAn = db7abba433f9f7aa789a21a7072a7d9e6bb2859d5de61db88008075f4b98959d11052023101555
    '---------------------   SHA ????  Collisioni   ----------------------------------------
    'sha256 = db7abba433f9f7aa789a21a7072a7d9e6bb2859d5de61db88008075f4b98959d
    '----- Devi trovare 2 file con lo stesso orario al SEC e lo stesso CONTENUTO -----------
    'CRC256 = db7abba433f9f7aa789a21a7072a7d9e6bb2859d5de61db88008075f4b98959d
    'DataOra= 11/05/2023 10:15:55                                             bbafcacdbabfff
    '---------------------------------  Se li trovi, sono uno la copia dell'altro. [RISOLTO]

    Public Shared Function DTCMaker(filename As String) As String
        ' Crea una stringa che rappresenta il CRC con la data di ultima modifica con l'orario
        '   aggiornati al secondo, quindi zero collisioni con l'orario
        ' Aggangiata alla SHA Sum del file, es:
        ' CRCAA  = db7abba433f9f7aa789a21a7072a7d9e6bb2859d5de61db88008075f4b98959dbbafcacdbabfff ' Zero collisioni
        ' CRCAAN = db7abba433f9f7aa789a21a7072a7d9e6bb2859d5de61db88008075f4b98959d11052023101555 ' Zero coll num.
        ' SHA256 = db7abba433f9f7aa789a21a7072a7d9e6bb2859d5de61db88008075f4b98959d               ' Consigliato
        Dim dtc As String = String.Empty

        Dim fs As FileInfo = New FileInfo(filename)
        Dim dMod As Date = fs.LastWriteTime '11/05/2023 10:15:55
        dtc = Strings.Replace(dMod.ToString, "/", "").Replace(":", "").Replace(" ", "")

        ' Possiamo convertire i numeri in lettere per camuffare lastwrite
        Dim wordsCRC As String = "abcdefghijklmnopqrstuvxywz"
        Dim dtcw As String = ""
        For j = 0 To dtc.Length - 1
            Dim c As String = dtc(j)
            dtcw &= wordsCRC.Substring(c, 1)
        Next

        ' Nuovo CRCAA zero collisioni
        Return CreateCRCAA(filename) & dtcw 'sempre 14
    End Function

    Public Shared Function DTCReader(CRCAA As String) As (String, String)
        ' Legge la data nel CRCAA,sempre 14 caratteri
        Dim Counter = 0
        Dim wordsCRC As String = "abcdefghijklmnopqrstuvxywz"
        Dim Start = CRCAA.Length - 14
        Dim Fine = CRCAA.Length - 1
        Dim dataCRC As String = ""
        For j = Start To Fine
            Dim c As String = CRCAA(j)
            dataCRC &= Strings.InStr(wordsCRC, c) - 1 'Parte da 0
            If Counter = 1 OrElse Counter = 3 Then dataCRC &= "/"
            If Counter = 7 Then dataCRC &= " "
            If Counter = 9 OrElse Counter = 11 Then dataCRC &= ":"
            Counter += 1
        Next

        Dim sha256 As String = CRCAA.Substring(0, 64) '64 caratteri
        Return (sha256, dataCRC)
    End Function

    Private Shared Function CreateCRCAA(ByVal filename As String) As String
        ' Crea un CRC di nuova generazione
        Using mySHA256 As SHA256 = SHA256.Create()
            Using mySHA512 As SHA512 = SHA512.Create()
                Using stream = File.OpenRead(filename)
                    ' Crea la stringa hash in esadecimale
                    Dim hash256 = HashToHex(mySHA256.ComputeHash(stream)) 'prima codifica
                    Dim hash512 = HashToHex(mySHA512.ComputeHash(stream))
                    Return hash256 ' prima codifica
                End Using
            End Using
        End Using
    End Function

    Private Shared Function HashToHex(hash As Byte()) As String
        ' Converte i bytes hash in HEX
        Return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant()
    End Function
End Class
