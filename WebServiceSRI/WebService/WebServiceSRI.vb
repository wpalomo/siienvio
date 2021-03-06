﻿Imports System.Text
Imports System.Xml
Imports System.Xml.Schema
Imports System.Net
Imports System.IO

Public Class WebServiceSRI

#Region "VARIABLES"

    Private recepcionValue As RecepcionComprobante = Nothing
    Private autorizacionValue As AutorizacionComprobante = Nothing
    Private tipoAmbienteValue As Integer
    Private tipoEmisionValue As Integer
    'Private tipoEmisionSoapValue As Integer = 1
    Private claveAccesoValue As String = String.Empty
    Private isOnLineValue As Boolean
    '------ Proxy ------
    Private blnUsarProxyValue As Boolean
    Private strDireccionProxyValue As String
    Private intPuertoProxyValue As UShort

#End Region

#Region "VARIABLES DE WEB SERVICE"

    Private Shared wsAutorizacionValue As New Dictionary(Of Integer, String)
    Private Shared wsRecepcionValue As New Dictionary(Of Integer, String)

#End Region

#Region "CONSTRUCTOR"

    Sub New(ByVal urlWsRecepcionProd As String, ByVal urlWsAutorizacionProd As String, ByVal urlWsRecepcionPru As String, ByVal urlWsAutoPru As String)
        loadWebSericeSRI(urlWsRecepcionProd, urlWsAutorizacionProd, urlWsRecepcionPru, urlWsAutoPru)
    End Sub

#End Region

#Region "PROPERTIES"

    Public Shared Property WSAutorizacion() As Dictionary(Of Integer, String)
        Get
            Return wsAutorizacionValue
        End Get
        Set(ByVal value As Dictionary(Of Integer, String))
            wsAutorizacionValue = value
        End Set
    End Property

    Public Shared Property WSRecepcion() As Dictionary(Of Integer, String)
        Get
            Return wsRecepcionValue
        End Get
        Set(ByVal value As Dictionary(Of Integer, String))
            wsRecepcionValue = value
        End Set
    End Property

    Public ReadOnly Property RecepcionComprobante() As RecepcionComprobante
        Get
            Return recepcionValue
        End Get
    End Property

    Public ReadOnly Property AutorizacionComprobante() As AutorizacionComprobante
        Get
            Return autorizacionValue
        End Get
    End Property

    Public ReadOnly Property ClaveAcceso() As String
        Get
            Return claveAccesoValue
        End Get
    End Property

    Public ReadOnly Property TipoAmbiente() As Integer
        Get
            Return tipoAmbienteValue
        End Get
    End Property

    Public ReadOnly Property TipoEmision() As Integer
        Get
            Return tipoEmisionValue
        End Get
    End Property

    'Public ReadOnly Property TipoEmisionSoap() As Integer
    '    Get
    '        Return tipoEmisionSoapValue
    '    End Get
    'End Property

    Public ReadOnly Property IsOnLine() As Boolean
        Get
            Return isOnLineValue
        End Get
    End Property

    Public Sub UsarProxy(ByVal blnValor As Boolean, ByVal strDireccionProxy As String, ByVal intPuertoProxy As UShort)
        blnUsarProxyValue = blnValor
        strDireccionProxyValue = strDireccionProxy
        intPuertoProxyValue = intPuertoProxy
    End Sub

#End Region

#Region "GET SOAP"

    Private Function getSoapValidacion(Optional ByVal xml As String = "") As String
        Dim str As New System.Text.StringBuilder
        '---------------------------------------------------------------------------------------------
        str.AppendLine("<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ec=""http://ec.gob.sri.ws.recepcion"">")
        str.AppendLine("<soapenv:Header/>")
        str.AppendLine("<soapenv:Body>")
        str.AppendLine("<ec:validarComprobante>")
        str.AppendFormat("<xml>{0}</xml>{1}", xml, vbCrLf)
        str.AppendLine("</ec:validarComprobante>")
        str.AppendLine("</soapenv:Body>")
        str.AppendLine("</soapenv:Envelope>")
        '---------------------------------------------------------------------------------------------
        getSoapValidacion = str.ToString
        '---------------------------------------------------------------------------------------------
        str = Nothing
    End Function

    Private Function getSoapAutorizacion(ByVal claveAccesoComprobante As String) As String
        Dim str As New System.Text.StringBuilder
        '---------------------------------------------------------------------------------------------
        str.AppendLine("<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ec=""http://ec.gob.sri.ws.autorizacion"">")
        str.AppendLine("<soapenv:Header/>")
        str.AppendLine("<soapenv:Body>")
        str.AppendLine("<ec:autorizacionComprobante>")
        str.AppendFormat("<claveAccesoComprobante>{0}</claveAccesoComprobante>{1}", claveAccesoComprobante, vbCrLf)
        str.AppendLine("</ec:autorizacionComprobante>")
        str.AppendLine("</soapenv:Body>")
        str.AppendLine("</soapenv:Envelope>")
        '---------------------------------------------------------------------------------------------
        getSoapAutorizacion = str.ToString
        '---------------------------------------------------------------------------------------------
        str = Nothing
    End Function

    Private Function getSoapAutorizacionLote(ByVal claveAccesoLote As String) As String
        Dim str As New System.Text.StringBuilder
        '---------------------------------------------------------------------------------------------
        str.AppendLine("<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ec=""http://ec.gob.sri.ws.autorizacion"">")
        str.AppendLine("<soapenv:Header/>")
        str.AppendLine("<soapenv:Body>")
        str.AppendLine("<ec:autorizacionComprobanteLote>")
        str.AppendFormat("<claveAccesoLote>{0}</claveAccesoLote>{1}", claveAccesoLote, vbCrLf)
        str.AppendLine("</ec:autorizacionComprobanteLote>")
        str.AppendLine("</soapenv:Body>")
        str.AppendLine("</soapenv:Envelope>")
        '---------------------------------------------------------------------------------------------
        getSoapAutorizacionLote = str.ToString
        '---------------------------------------------------------------------------------------------
        str = Nothing
    End Function

    Private Function getSoapAutorizacionLoteMasivo(ByVal claveAccesoLote As String) As String
        Dim str As New System.Text.StringBuilder
        '---------------------------------------------------------------------------------------------
        str.AppendLine("<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ec=""http://ec.gob.sri.ws.autorizacion"">")
        str.AppendLine("<soapenv:Header/>")
        str.AppendLine("<soapenv:Body>")
        str.AppendLine("<ec:autorizacionComprobanteLoteMasivo>")
        str.AppendFormat("<claveAccesoLote>{0}</claveAccesoLote>{1}", claveAccesoLote, vbCrLf)
        str.AppendLine("</ec:autorizacionComprobanteLoteMasivo>")
        str.AppendLine("</soapenv:Body>")
        str.AppendLine("</soapenv:Envelope>")
        '---------------------------------------------------------------------------------------------
        getSoapAutorizacionLoteMasivo = str.ToString
        '---------------------------------------------------------------------------------------------
        str = Nothing
    End Function

#End Region

#Region "SEND COMPROBANTE"
    'Metodo para enviar el comprobante al SRI, cosume los ws de recepcion y autorizacion
    Public Function enviarComprobanteSRIPorNombreArchivo(ByVal fileName As String) As Boolean
        'toma el  archivo generado y que ya esta firmado
        Dim xmlString As String = getOuterXML(fileName)
        Dim retVal As Boolean
        'validar que el archivo no este vacio
        If xmlString <> String.Empty Then
            'VERIFICO QUE EXISTA CONEXION A INTERNET
            If My.Computer.Network.IsAvailable = True Then
                resetVariables()
                'LLOS DATOS IMPORTANTES DEL ARCHIVO XML
                readElements(xmlString)
                retVal = enviarComprobanteSRIOffLine(Metodos.ToBase64String(xmlString))
            End If
        End If
        enviarComprobanteSRIPorNombreArchivo = retVal
    End Function

#End Region

#Region "PROCESS SEND COMPROBANTE"
    'Metodo para ejecutar el ws de recepcion del SRI
    Private Function enviarRecepcionComprobante(ByVal VALUE As String, Optional ByVal ENVIO As EnvioType = EnvioType.NORMAL) As RecepcionComprobante
        enviarRecepcionComprobante = New RecepcionComprobante(executeWebServiceCLIENT(VALUE, AcctionType.RECEPCION, ENVIO))
    End Function
    'Metodo para ejecutar el ws de autorizacion  del SRI
    Private Function enviarAutorizacionComprobante(ByVal VALUE As String, Optional ByVal ENVIO As EnvioType = EnvioType.NORMAL) As AutorizacionComprobante
        enviarAutorizacionComprobante = New AutorizacionComprobante(executeWebServiceCLIENT(VALUE, AcctionType.AUTORIZACION, ENVIO))
    End Function
    'Metodo que  utiliza el ws del SRI de recepcion para  enviar el xml
    <Obsolete("Deprecado ,  utilizar el metodo Offline")>
    Private Function enviarComprobanteSRI(ByVal VALUE As String, Optional ByVal ENVIO As EnvioType = EnvioType.NORMAL) As Boolean
        Dim retVal As Boolean
        '--------------------------------------------------------------------------------------------------
        isOnLineValue = False
        ''tipoEmisionSoapValue = 0
        '--------------------------------------------------------------------------------------------------
        recepcionValue = New RecepcionComprobante
        'autorizacionValue = New AutorizacionComprobante
        '--------------------------------------------------------------------------------------------------
        'verifico sino ha sido autorizado anteriormente o esta pendiente de autorizar
        '--------------------------------------------------------------------------------------------------
        autorizacionValue = enviarAutorizacionComprobante(claveAccesoValue, ENVIO)
        '--------------------------------------------------------------------------------------------------
        'verifico si hubo conexion con los webservices del sri
        '--------------------------------------------------------------------------------------------------
        If autorizacionValue.SoapString <> String.Empty Then
            isOnLineValue = True
            'Solo si el comprobante ya no esta autorizado--------------------------------------------------
            If autorizacionValue.IsAutorizado = False Then
                '------------------------------------------------------------------------------------------
                'verifico si esta pendiente de Autorizar
                '------------------------------------------------------------------------------------------
                If autorizacionValue.IsPendiente = False Then
                    recepcionValue = enviarRecepcionComprobante(VALUE, ENVIO)
                    If recepcionValue.SoapString <> String.Empty Then
                        If recepcionValue.IsRecibida = True Then
                            autorizacionValue = enviarAutorizacionComprobante(claveAccesoValue, ENVIO)
                            '------------------------------------------------------------------------------
                            If autorizacionValue.NumeroComprobantes <= 0 Then
                                autorizacionValue = enviarAutorizacionComprobante(claveAccesoValue, ENVIO)
                            End If
                            '------------------------------------------------------------------------------
                            If autorizacionValue.SoapString = String.Empty Then
                                ''tipoEmisionSoapValue = 2
                            End If
                            '------------------------------------------------------------------------------
                            retVal = True
                        End If
                    Else
                        ''tipoEmisionSoapValue = 2
                    End If
                End If
            Else
                retVal = True
            End If
        Else
            ''tipoEmisionSoapValue = 2
        End If
        '---------------------------------------------------------------------------------------------------
        ''If tipoEmisionSoapValue = 0 Then
        ''    tipoEmisionSoapValue = tipoEmisionValue
        ''End If
        '---------------------------------------------------------------------------------------------------
        enviarComprobanteSRI = retVal
    End Function
    '@autor aquingaluisa
    'Metodo que  utiliza el ws del SRI de recepcion para  enviar el xml
    Private Function enviarComprobanteSRIOffLine(ByVal VALUE As String, Optional ByVal ENVIO As EnvioType = EnvioType.NORMAL) As Boolean
        Dim retVal As Boolean
        '--------------------------------------------------------------------------------------------------
        isOnLineValue = False
        recepcionValue = New RecepcionComprobante
        'verifico sino ha sido autorizado anteriormente o esta pendiente de autorizar
        autorizacionValue = enviarAutorizacionComprobante(claveAccesoValue, ENVIO)
        'verifico si hubo conexion con los webservices del sri
        If autorizacionValue.SoapString <> String.Empty Then
            isOnLineValue = True
            'Solo si el comprobante ya no esta autorizado--------------------------------------------------
            If autorizacionValue.IsAutorizado = False Then
                'verifico si esta pendiente de Autorizar
                If autorizacionValue.IsPendiente = False Then
                    recepcionValue = enviarRecepcionComprobante(VALUE, ENVIO)
                    If recepcionValue.SoapString <> String.Empty Then
                        If recepcionValue.IsRecibida = True Then
                            autorizacionValue = enviarAutorizacionComprobante(claveAccesoValue, ENVIO)

                            If autorizacionValue.NumeroComprobantes <= 0 Then
                                autorizacionValue = enviarAutorizacionComprobante(claveAccesoValue, ENVIO)
                            End If
                            retVal = True
                        End If
                    End If
                End If
            Else
                retVal = True
            End If
        End If
        enviarComprobanteSRIOffLine = retVal
    End Function

#End Region

#Region "RESPUESTA DE WEB SERVICE"

    Private Function executeWebServiceCLIENT(ByVal VALUE As String, ByVal ACCTION As AcctionType, Optional ByVal ENVIO As EnvioType = EnvioType.NORMAL) As String
        Dim w As New WebClient
        Dim data As String = String.Empty
        Dim address As String = String.Empty
        Dim retVal As String = String.Empty
        If tipoAmbienteValue <> 1 And tipoAmbienteValue <> 2 Then
            Throw New System.Exception("El ambiente del archivo no es el correcto, valor ambiente:" & tipoAmbienteValue)
        End If

        '---------------------------------------------------------------------------------------------
        If ACCTION = AcctionType.AUTORIZACION Then
            address = WSAutorizacion(tipoAmbienteValue)
        Else
            address = WSRecepcion(tipoAmbienteValue)
        End If
        '---------------------------------------------------------------------------------------------
        If ACCTION = AcctionType.AUTORIZACION Then
            If ENVIO = EnvioType.NORMAL Then
                data = getSoapAutorizacion(VALUE)
            ElseIf ENVIO = EnvioType.LOTE Then
                data = getSoapAutorizacionLote(VALUE)
            ElseIf ENVIO = EnvioType.LOTE_MASIVO Then
                data = getSoapAutorizacionLoteMasivo(VALUE)
            End If
        Else
            data = getSoapValidacion(VALUE)
        End If
        '---------------------------------------------------------------------------------------------
        w.Encoding = System.Text.Encoding.UTF8
        '---------------------------------------------------------------------------------------------
        'SI DESEAS USAR PROXY DEBERIAS HACER LO SIGUIENTE
        '---------------------------------------------------------------------------------------------
        If blnUsarProxyValue = True Then
            'Dim cr As New System.Net.NetworkCredential("smtp", "123456.a", "hormipisos.com")
            Dim pr As New System.Net.WebProxy(strDireccionProxyValue, CInt(intPuertoProxyValue))
            pr.Credentials = System.Net.CredentialCache.DefaultCredentials
            'pr.Credentials = cr
            w.Proxy = pr
        Else
            'w.Proxy
        End If
        '---------------------------------------------------------------------------------------------
        Try
            retVal = w.UploadString(address, data)
        Catch ex As WebException
            'MsgBox(ex.Message, MsgBoxStyle.Critical, "executeWebServiceCLIENT")
        End Try
        '---------------------------------------------------------------------------------------------
        executeWebServiceCLIENT = retVal
        '---------------------------------------------------------------------------------------------
        w.Dispose()
        '---------------------------------------------------------------------------------------------
        w = Nothing
    End Function

#End Region

#Region "METODOS"

    Private Sub resetVariables()
        tipoEmisionValue = 0
        tipoAmbienteValue = 0
        isOnLineValue = False
        claveAccesoValue = String.Empty
        recepcionValue = Nothing
        autorizacionValue = Nothing
    End Sub

    Private Sub readElements(ByVal xmlString As String)
        Dim x As New System.Xml.XmlDocument
        '--------------------------------------------------------------------------------------------------
        x.PreserveWhitespace = True
        '--------------------------------------------------------------------------------------------------
        x.LoadXml(xmlString)
        '--------------------------------------------------------------------------------------------------
        readElements(x.DocumentElement)
        '--------------------------------------------------------------------------------------------------
        x = Nothing
    End Sub
    'Leer los nodos del xml
    Private Sub readElements(ByVal node As XmlNode)
        For Each nodeChild As XmlNode In node.ChildNodes
            If nodeChild.NodeType <> XmlNodeType.Text Then
                If nodeChild.LocalName.Contains("claveAcceso") = True Then
                    claveAccesoValue = nodeChild.InnerText
                ElseIf nodeChild.LocalName.Contains("ambiente") = True Then
                    tipoAmbienteValue = CInt(nodeChild.InnerText)
                ElseIf nodeChild.LocalName.Contains("tipoEmision") = True Then
                    tipoEmisionValue = CInt(nodeChild.InnerText)
                End If
                '-----------------------------------------------------------
                If nodeChild.ChildNodes.Count > 0 Then
                    If claveAccesoValue = String.Empty Or tipoAmbienteValue = 0 Or tipoEmisionValue = 0 Then
                        readElements(nodeChild)
                    End If
                End If
            End If
        Next
    End Sub

    Private Sub loadWebSericeSRI(ByVal urlWsRecepcionProd As String, ByVal urlWsAutorizacionProd As String, ByVal urlWsRecepcionPru As String, ByVal urlWsAutorizacionPru As String)
        Dim wsAutValue As New Dictionary(Of Integer, String)
        Dim wsRecValue As New Dictionary(Of Integer, String)
        '--------------------------------------------------------------------------
        If wsAutorizacionValue.Count <= 0 Or wsRecepcionValue.Count <= 0 Then
            wsAutValue.Add(1, urlWsAutorizacionPru)
            wsRecValue.Add(1, urlWsRecepcionPru)
            '----------------------------------------------------------------------
            wsAutValue.Add(2, urlWsAutorizacionProd)
            wsRecValue.Add(2, urlWsRecepcionProd)
            '----------------------------------------------------------------------
            wsAutorizacionValue = wsAutValue
            wsRecepcionValue = wsRecValue
        End If
        '--------------------------------------------------------------------------
        wsAutValue = Nothing
        wsRecValue = Nothing
    End Sub

    Public Function IsAccesibleWS(Optional ByVal tipoAmbiente As AmbienteType = AmbienteType.PRODUCCION) As Boolean
        Dim w As New WebClient
        Dim s As String = String.Empty
        '--------------------------------------------------------------------------------
        Try
            s = w.UploadString(WSRecepcion(CInt(tipoAmbiente)), getSoapValidacion())
        Catch ex As WebException
            Console.WriteLine(ex.Message)
        End Try
        '--------------------------------------------------------------------------------
        IsAccesibleWS = (s <> String.Empty)
        '--------------------------------------------------------------------------------
        w.Dispose()
        '--------------------------------------------------------------------------------
        w = Nothing
    End Function

    Public Shared Function getOuterXML(ByVal fileNameXML As String) As String
        Dim reader As XmlTextReader = Nothing
        Dim xml As New XmlDocument
        Dim retVal As String = String.Empty
        '--------------------------------------------------------------------------------------------------------
        If fileNameXML <> String.Empty Then
            reader = New XmlTextReader(fileNameXML)
            '----------------------------------------------------------------------------------------------------
            xml.PreserveWhitespace = True
            '----------------------------------------------------------------------------------------------------
            xml.Load(reader)
            '----------------------------------------------------------------------------------------------------
            retVal = xml.OuterXml
            '----------------------------------------------------------------------------------------------------
            reader.Close()
        End If
        '--------------------------------------------------------------------------------------------------------
        getOuterXML = retVal
        '--------------------------------------------------------------------------------------------------------
        xml = Nothing
        reader = Nothing
    End Function

#End Region

End Class