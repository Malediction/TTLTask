﻿using System;
using System.IO;

namespace AddressProcessing.CSV
{
    /*
        1) List three to five key concerns with this implementation that you would discuss with the junior developer. 

        Please leave the rest of this file as it is so we can discuss your concerns during the next stage of the interview process.
        
        *) The design is overloaded and needlessly complex, violates SRP and DRY principles. The fact it has flags indicating behaviour is (pun unintended) a red flag that it 
        * should be split into subcomponents that perhaps share a common base class. This would be a breaking change however.
        
        *) The class does not implement IDisposable despite having fields which manager native OS resources (file handles).
        
        *) The first overload of Read() doesn't appear to understand the references are passed by value in the CLR, so the values for column1 and column2 are not 
        * going to be visible to the caller. Is this function purely meant to be used for skipping a line? It should have a different name and no arguments if so.
        * This would be a breaking change however.
        
        *) The second overload of Read() is full of copy and paste from the first overload. If the two overloads are to be preserved, refactor them to use common functions.
        * They should use "out" parameters so that the result values are passed to the caller and to ask the compiler to make sure the references are set before the end of the function
        
        *) Write() should use a StringBuilder for its output buffer to reduce GC churn via too many string reallocs.
        
        *) Write()'s entire implemetation could be replaced with a call to string.Join("\t", columns) and just pass the result through to WriteLine().
        
        These are more minor than my other suggestions, but I thought I'd include them as I would when reviewing code anyway:

        *) The exception message in Open() is unhelpful, it should at least tell you the integer value of the underlying enum so the caller can try and root cause what invalid value they've
        * passed in (likely a default(T) value of 0 from an ORM or something similar)        

        *) The class should not have an "Open" method and instead just allocate streams in the constructor and tidy them up in Dispose() (perhaps with a call to GC.SupressFinalize() too). 
        * This would be a breaking change however.

        *) Read() should probably be called TryRead() to match .NET naming conventions for functions with side effects that may fail. This would be a breaking change however.

        *) The class appears to be reading and writing TSV data but this class is meant to be a CSV reader/writer? Bad name?
                
        *) Bad variable name casing, "output", not "outPut". puts() is C, not C#.
    */

    public class CSVReaderWriterForAnnotation
    {
        private StreamReader _readerStream = null;
        private StreamWriter _writerStream = null;

        [Flags]
        public enum Mode { Read = 1, Write = 2 };

        public void Open(string fileName, Mode mode)
        {
            if (mode == Mode.Read)
            {
                _readerStream = File.OpenText(fileName);
            }
            else if (mode == Mode.Write)
            {
                FileInfo fileInfo = new FileInfo(fileName);
                _writerStream = fileInfo.CreateText();
            }
            else
            {
                throw new Exception("Unknown file mode for " + fileName);
            }
        }

        public void Write(params string[] columns)
        {
            string outPut = "";

            for (int i = 0; i < columns.Length; i++)
            {
                outPut += columns[i];
                if ((columns.Length - 1) != i)
                {
                    outPut += "\t";
                }
            }
            
            WriteLine(outPut);
        }

        public bool Read(string column1, string column2)
        {
            const int FIRST_COLUMN = 0;
            const int SECOND_COLUMN = 1;

            string line;
            string[] columns;

            char[] separator = { '\t' };

            line = ReadLine();
            columns = line.Split(separator);

            if (columns.Length == 0)
            {
                column1 = null;
                column2 = null;

                return false;
            }
            else
            {
                column1 = columns[FIRST_COLUMN];
                column2 = columns[SECOND_COLUMN];

                return true;
            }
        }

        public bool Read(out string column1, out string column2)
        {
            const int FIRST_COLUMN = 0;
            const int SECOND_COLUMN = 1;

            string line;
            string[] columns;

            char[] separator = { '\t' };

            line = ReadLine();

            if (line == null)
            {
                column1 = null;
                column2 = null;

                return false;
            }

            columns = line.Split(separator);

            if (columns.Length == 0)
            {
                column1 = null;
                column2 = null;

                return false;
            } 
            else
            {
                column1 = columns[FIRST_COLUMN];
                column2 = columns[SECOND_COLUMN];

                return true;
            }
        }

        private void WriteLine(string line)
        {
            _writerStream.WriteLine(line);
        }

        private string ReadLine()
        {
            return _readerStream.ReadLine();
        }

        public void Close()
        {
            if (_writerStream != null)
            {
                _writerStream.Close();
            }

            if (_readerStream != null)
            {
                _readerStream.Close();
            }
        }
    }
}
