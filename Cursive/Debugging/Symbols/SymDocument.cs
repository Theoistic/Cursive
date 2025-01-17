//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------


// These interfaces serve as an extension to the BCL's SymbolStore interfaces.
namespace Cursive.Debugging.CorSymbolStore 
{
    using System;
    using System.Diagnostics.SymbolStore;
    using System.Runtime.InteropServices;
    using System.Text;
	
    [
        ComImport,
        Guid("40DE4037-7C81-3E1E-B022-AE1ABFF2CA08"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        ComVisible(false)
    ]
    internal interface ISymUnmanagedDocument
    {
        void GetURL(int cchUrl,
                       out int pcchUrl,
                       [MarshalAs(UnmanagedType.LPWStr)] StringBuilder szUrl);
     
        void GetDocumentType(ref Guid pRetVal);
    
        void GetLanguage(ref Guid pRetVal);
    
        void GetLanguageVendor(ref Guid pRetVal);
    
        void GetCheckSumAlgorithmId(ref Guid pRetVal);
    
        void GetCheckSum(int cData,
                              out int pcData,
                              [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] byte[] data);
     
        void FindClosestLine(int line,
                                out int pRetVal);
    
        void HasEmbeddedSource(out Boolean pRetVal);
    
        void GetSourceLength(out int pRetVal);
    
        void GetSourceRange(int startLine,
                                 int startColumn,
                                 int endLine,
                                 int endColumn,
                                 int cSourceBytes,
                                 out int pcSourceBytes,
                                 [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] byte[] source);
     
    };
    
    
    internal class SymbolDocument : ISymbolDocument
    {
        ISymUnmanagedDocument m_unmanagedDocument;
        
        internal SymbolDocument(ISymUnmanagedDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }
            m_unmanagedDocument = document;
        }
        
        
        public String URL 
        { 
            get
            {
                StringBuilder URL;
                int cchUrl;
                m_unmanagedDocument.GetURL(0, out cchUrl, null);
                URL = new StringBuilder(cchUrl);
                m_unmanagedDocument.GetURL(cchUrl, out cchUrl, URL);
                return URL.ToString();
            }
        }

        
        public Guid DocumentType 
        { 
            get
            {
                Guid guid = new Guid();
                m_unmanagedDocument.GetDocumentType(ref guid);
                return guid;
            }
        }

        
        public Guid Language 
        { 
            get
            {
                Guid guid = new Guid();
                m_unmanagedDocument.GetLanguage(ref guid);
                return guid;
            }
        }

        
        public Guid LanguageVendor
        { 
            get
            {
                Guid guid = new Guid();
                m_unmanagedDocument.GetLanguageVendor(ref guid);
                return guid;
            }
        }

        
        public Guid CheckSumAlgorithmId
        { 
            get
            {
                Guid guid = new Guid();
                m_unmanagedDocument.GetCheckSumAlgorithmId(ref guid);
                return guid;
            }
        }

        
        public byte[] GetCheckSum()
        {
            byte[] Data;
            int cData = 0;
            m_unmanagedDocument.GetCheckSum(0, out cData, null);
            Data = new byte[cData];
            m_unmanagedDocument.GetCheckSum(cData, out cData, Data);
            return Data;
        }
        

        
        public int FindClosestLine(int line)
        {
            int closestLine = 0;
            m_unmanagedDocument.FindClosestLine(line, out closestLine);
            return closestLine;
        }

        
        public bool HasEmbeddedSource 
        { 
            get
            {
                bool retVal = false;
                m_unmanagedDocument.HasEmbeddedSource(out retVal);
                return retVal;
            }
        }

        
        public int SourceLength
        { 
            get
            {
                int retVal = 0;
                m_unmanagedDocument.GetSourceLength(out retVal);
                return retVal;
            }
        }
            
        

        
        public byte[] GetSourceRange(int startLine, int startColumn,
                                          int endLine, int endColumn)
        {                                    
            byte[] Data;
            int count = 0;
            m_unmanagedDocument.GetSourceRange(startLine, startColumn, endLine, endColumn, 0, out count, null);
            Data = new byte[count];
            m_unmanagedDocument.GetSourceRange(startLine, startColumn, endLine, endColumn, count, out count, Data);
            return Data;
        }
                                      
        internal ISymUnmanagedDocument InternalDocument
        {
            get
            {
                return m_unmanagedDocument;
            }
        }
                                      
    }
}
