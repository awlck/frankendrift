Friend Class AutoCompleteSortedArrayList
    Inherits Generic.List(Of AutoComplete) ' ArrayList

    ' We want to store values by arbritray key, so need a hashtable to lookup the index in the arraylist
    Private htblIndexLookup As New Hashtable

    Public Shadows Sub Add(ByVal ac As AutoComplete)
        MyBase.Add(ac)
        htblIndexLookup.Add(ac.sWord, MyBase.Count - 1)
    End Sub

    Public Shadows Sub Remove(ByVal key As String)
        MyBase.RemoveAt(CInt(htblIndexLookup(key)))
        htblIndexLookup.Remove(key)
    End Sub

    Public Shadows Sub Clear()
        MyBase.Clear()
        htblIndexLookup.Clear()
    End Sub

    Public ReadOnly Property ItemByIndex(ByVal index As Integer) As AutoComplete
        Get
            Dim sKey As String = GetKeyFromIndex(index)
            Return Item(sKey)
        End Get
    End Property

    Default Public Shadows Property Item(ByVal key As String) As AutoComplete
        Get
            Return CType(MyBase.Item(CInt(htblIndexLookup(key))), AutoComplete)
        End Get
        Set(ByVal value As AutoComplete)
            MyBase.Item(CInt(htblIndexLookup(key))) = value
        End Set
    End Property

    Public Function ContainsKey(ByVal key As String) As Boolean
        Return htblIndexLookup.ContainsKey(key)
    End Function

    Public Function GetKeyFromIndex(ByVal index As Integer) As String
        Return CType(MyBase.Item(index), AutoComplete).sWord
    End Function

    Public Shadows Sub Sort()

        MyBase.Sort()
        htblIndexLookup.Clear()
        For i As Integer = 0 To MyBase.Count - 1
            htblIndexLookup.Add(CType(MyBase.Item(i), AutoComplete).sWord, i)
        Next

    End Sub

End Class

Friend Class AutoComplete
    Implements IComparable(Of AutoComplete)

    Public sWord As String
    Public iPriority As Integer = Integer.MaxValue
    Friend htblChildren As New AutoCompleteSortedArrayList ' Hashtable

    Public salTasks As New StringArrayList

    Public Sub New()
        MyBase.New()
    End Sub
    Public Sub New(ByVal sWord As String)
        MyBase.New()
        Me.sWord = sWord
    End Sub

    ' Order these by:
    '   Auto-complete priority
    '   Most common (i.e. most children)
    '   Longest word (if same task)
    '   Name
    ' Order these by most children, so most common words appear first
    Public Function CompareTo(ByVal ac As AutoComplete) As Integer Implements System.IComparable(Of AutoComplete).CompareTo
        If iPriority <> ac.iPriority Then
            Return iPriority.CompareTo(ac.iPriority)
        Else
            If salTasks.Count = ac.salTasks.Count Then
                If sWord.Length = ac.sWord.Length Then
                    Return sWord.CompareTo(ac.sWord)
                Else
                    Return ac.sWord.Length.CompareTo(sWord.Length)
                End If
            Else
                Return ac.salTasks.Count.CompareTo(salTasks.Count)
            End If
        End If
    End Function
End Class