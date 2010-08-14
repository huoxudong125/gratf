﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Xml;

namespace GlyphRecognitionStudio
{
    class Configuration
    {
        private static Configuration singleton = null;

        private const string baseConfigFileName = "glyph recognition studio.cfg";
        private string configFileName = null;
        bool isSuccessfullyLoaded = false;

        #region XML Tag Names
        private const string mainTag = "GlyphRecognitionStudio";
        private const string glyphDatabasesTag = "GlyphDatabases";
        #endregion

        // Configuration load status
        public bool IsSuccessfullyLoaded
        {
            get { return isSuccessfullyLoaded; }
        }

        // Disable making class instances
        private Configuration( )
        {
            configFileName = Path.Combine(
                Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                baseConfigFileName );
        }

        // Get instance of the configuration storage
        public static Configuration Instance
        {
            get
            {
                if ( singleton == null )
                {
                    singleton = new Configuration( );
                }
                return singleton;
            }
        }

        // Save application's configuration
        public void Save( GlyphDatabases dbs )
        {
            lock ( baseConfigFileName )
            {
                try
                {
                    // open file
                    FileStream fs = new FileStream( configFileName, FileMode.Create );
                    // create XML writer
                    XmlTextWriter xmlOut = new XmlTextWriter( fs, Encoding.UTF8 );

                    // use indenting for readability
                    xmlOut.Formatting = Formatting.Indented;

                    // start document
                    xmlOut.WriteStartDocument( );
                    xmlOut.WriteComment( "Glyph Recognition Studio configuration file" );

                    // main node
                    xmlOut.WriteStartElement( mainTag );

                    // save glyph databases
                    xmlOut.WriteStartElement( glyphDatabasesTag );
                    dbs.Save( xmlOut );
                    xmlOut.WriteEndElement( );

                    xmlOut.WriteEndElement( ); // end of main node
                    // close file
                    xmlOut.Close( );
                }
                catch
                {
                }
            }
        }

        // Load application's configration
        public bool Load( GlyphDatabases dbs )
        {
            isSuccessfullyLoaded = false;

            lock ( baseConfigFileName )
            {
                // check file existance
                if ( File.Exists( configFileName ) )
                {
                    FileStream fs = null;
                    XmlTextReader xmlIn = null;

                    try
                    {
                        // open file
                        fs = new FileStream( configFileName, FileMode.Open );
                        // create XML reader
                        xmlIn = new XmlTextReader( fs );

                        xmlIn.WhitespaceHandling = WhitespaceHandling.None;
                        xmlIn.MoveToContent( );

                        // check main node
                        if ( xmlIn.Name != mainTag )
                            throw new ApplicationException( );

                        // move to next node
                        xmlIn.Read( );

                        while ( true )
                        {
                            // ignore anything if it is not under main tag
                            while ( ( xmlIn.Depth > 1 ) || (
                                    ( xmlIn.Depth == 1 ) && ( xmlIn.NodeType != XmlNodeType.Element ) ) )
                            {
                                xmlIn.Read( );
                            }

                            // break if end element is reached
                            if ( xmlIn.Depth == 0 )
                                break;

                            int tagStartLineNummber = xmlIn.LineNumber;

                            switch ( xmlIn.Name )
                            {
                                case glyphDatabasesTag:
                                    dbs.Load( xmlIn );
                                    break;
                            }

                            // read to the next node, if loader did not move any further
                            if ( xmlIn.LineNumber == tagStartLineNummber )
                            {
                                xmlIn.Read( );
                            }
                        }

                        isSuccessfullyLoaded = true;
                        // ignore the rest
                    }
                    catch
                    {
                    }
                    finally
                    {
                        if ( xmlIn != null )
                            xmlIn.Close( );
                    }
                }
            }

            return isSuccessfullyLoaded;
        }
    }
}
