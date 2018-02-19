using System.Collections;
using UnityEngine;
using System.IO;

/**
 * The following Code was inspired and partly copied from the following two sources:
 * SUSHANTA CHAKRABORTY, "How to write data to CSV file in UNITY", February 20 2015, https://sushanta1991.blogspot.de/2015/02/how-to-write-data-to-csv-file-in-unity.html
 * shanemartin2797, "HOW TO READ FROM AND WRITE TO CSV IN UNITY", November 20, 2015, https://shanemartin2797blog.wordpress.com/2015/11/20/how-to-read-from-and-write-to-csv-in-unity/
 * 
 **/

public class CSVParser : MonoBehaviour {

    #region Private variables
    private StreamWriter _writer = null;

    private bool _csvAlreadyThere = false;

    private int _numberCSVColumns = 0;
    private int _writtenRowData = 0;

    private string _fileName;
    #endregion

    /** 
     * Instanciates the CSV parser 
     **/
    public CSVParser(string file)
    {
        _fileName = file;
    }


    /**
     * Instanciates the file writer and get's the path to the output file
     **/
    public void Start()
    {
        string path = getPath(_fileName);
        _csvAlreadyThere = File.Exists(path);
        _writer = new StreamWriter(path, true);
    }

    /**
     * Writes the header of the CSV file if the file is emtpy.
     * Note that the fields must be seperated by comma.
     **/
    public void writeHeader(string header, int numberColumns)
    {
        _numberCSVColumns = numberColumns;
        if (!_csvAlreadyThere)
        {
            _writer.Write(header);
            _writer.Flush();
        }
    }

    /**
     * Add new values to the file and automatically starts a new line if as many values were added as the spezified header has columns.
     * If the flag finishedWriting is set to true, the writer is closed.
     **/
    public bool appendValues(string value, int number = 1, bool finishedWriting = false)
    {
        if (_writtenRowData == 0)
        {
            _writer.Write("\r\n" + value);
        }
        else
        {
            _writer.Write("," + value);
        }
        _writer.Flush();

        // Sets the number of written data to 0 if as many values where written as the header contains columns
        // Otherwise the number of written rows is updated
        _writtenRowData = ((_writtenRowData + number) == _numberCSVColumns) ? 0 : (_writtenRowData + number);

        if (finishedWriting)
        {
            _writer.Close();
            return true;
        }
        return false;
    }

    /**
     * Use Platform #define directives to retrieve the path for the CSV file depending on the used platform.
     **/
    private string getPath(string file)
    {
        #if UNITY_EDITOR
            // If the directory CSV doesn't exist in Assets yet, it is created
            if (!Directory.Exists(Application.dataPath + "/CSV/"))
                {
                 Directory.CreateDirectory(Application.dataPath + "/CSV/");
                }
            return Application.dataPath + "/CSV/" + file;
        #else
            return Application.dataPath +"/"+file;
        #endif

    }

}
