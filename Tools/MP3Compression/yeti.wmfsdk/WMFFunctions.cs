//Widows Media Format Functions
//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  yetiicb@hotmail.com
//
//  Copyright (C) 2002-2004 Idael Cardoso. 
//

using System;
using System.Runtime.InteropServices;

namespace Yeti.WMFSdk
{
	/// <summary>
	/// Helper class that define the Windows Media Format Functions and constants
	/// </summary>
	public sealed class WM
	{
		private WM() {}

    [DllImport("WMVCore.dll", EntryPoint="WMCreateReader",  SetLastError=true,
     CharSet=CharSet.Unicode, ExactSpelling=true,
     CallingConvention=CallingConvention.StdCall)] 
    private static extern int WMCreateReader( IntPtr pUnkReserved,
                                              WMT_RIGHTS dwRights,
                                              [Out, MarshalAs(UnmanagedType.Interface)] out IWMReader ppReader);
    
    [DllImport("WMVCore.dll", EntryPoint="WMCreateSyncReader",  SetLastError=true,
       CharSet=CharSet.Unicode, ExactSpelling=true,
       CallingConvention=CallingConvention.StdCall)] 
    private static extern int WMCreateSyncReader(IntPtr pUnkCert,
                                                 WMT_RIGHTS dwRights,
                                                 [Out, MarshalAs(UnmanagedType.Interface)] out IWMSyncReader ppSyncReader);
    
    [DllImport("WMVCore.dll", EntryPoint="WMCreateSyncReader",  SetLastError=true,
       CharSet=CharSet.Unicode, ExactSpelling=true,
       CallingConvention=CallingConvention.StdCall)] 
    private static extern int WMCreateProfileManager( [Out, MarshalAs(UnmanagedType.Interface)] out IWMProfileManager  ppProfileManager );

    [DllImport("WMVCore.dll", EntryPoint="WMCreateEditor",  SetLastError=true,
       CharSet=CharSet.Unicode, ExactSpelling=true,
       CallingConvention=CallingConvention.StdCall)] 
    private static extern int WMCreateEditor([Out, MarshalAs(UnmanagedType.Interface)] out IWMMetadataEditor ppEditor);

    [DllImport("WMVCore.dll", EntryPoint="WMCreateIndexer",  SetLastError=true,
       CharSet=CharSet.Unicode, ExactSpelling=true,
       CallingConvention=CallingConvention.StdCall)] 
    private static extern int WMCreateIndexer([Out, MarshalAs(UnmanagedType.Interface)] out IWMIndexer  ppIndexer);
    
    [DllImport("WMVCore.dll", EntryPoint="WMCreateWriter",  SetLastError=true,
       CharSet=CharSet.Unicode, ExactSpelling=true,
       CallingConvention=CallingConvention.StdCall)] 
    private static extern int WMCreateWriter(IntPtr pUnkReserved, [Out, MarshalAs(UnmanagedType.Interface)] out IWMWriter ppWriter);


    [DllImport("WMVCore.dll", EntryPoint="WMCreateWriterFileSink",  SetLastError=true,
       CharSet=CharSet.Unicode, ExactSpelling=true,
       CallingConvention=CallingConvention.StdCall)] 
    private static extern int WMCreateWriterFileSink([Out, MarshalAs(UnmanagedType.Interface)] out IWMWriterFileSink  ppSink);

    [DllImport("WMVCore.dll", EntryPoint="WMCreateWriterNetworkSink",  SetLastError=true,
       CharSet=CharSet.Unicode, ExactSpelling=true,
       CallingConvention=CallingConvention.StdCall)] 
    private static extern int WMCreateWriterNetworkSink([Out, MarshalAs(UnmanagedType.Interface)] out IWMWriterNetworkSink ppSink);

    [DllImport("WMVCore.dll", EntryPoint="WMCreateWriterPushSink",  SetLastError=true,
       CharSet=CharSet.Unicode, ExactSpelling=true,
       CallingConvention=CallingConvention.StdCall)] 
    private static extern int WMCreateWriterPushSink([Out, MarshalAs(UnmanagedType.Interface)] out IWMWriterPushSink ppSink);

    [DllImport("WMVCore.dll", EntryPoint="WMIsAvailableOffline",  SetLastError=true,
       CharSet=CharSet.Unicode, ExactSpelling=true,
       CallingConvention=CallingConvention.StdCall)] 
    private static extern int WMIsAvailableOffline( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
                                                    [In, MarshalAs(UnmanagedType.LPWStr)] string pwszLanguage,
                                                    [Out, MarshalAs(UnmanagedType.Bool)] out bool pfIsAvailableOffline);

    [DllImport("WMVCore.dll", EntryPoint="WMIsContentProtected",  SetLastError=true,
       CharSet=CharSet.Unicode, ExactSpelling=true,
       CallingConvention=CallingConvention.StdCall)] 
    private static extern int WMIsContentProtected([In, MarshalAs(UnmanagedType.LPWStr)] string pwszFileName,
                                                   [Out, MarshalAs(UnmanagedType.Bool)] out bool pfIsProtected );

    [DllImport("WMVCore.dll", EntryPoint="WMValidateData",  SetLastError=true,
       CharSet=CharSet.Unicode, ExactSpelling=true,
       CallingConvention=CallingConvention.StdCall)] 
    private static extern int WMValidateData( [In, MarshalAs(UnmanagedType.LPArray)] byte[] pbData,
                                              [In, Out] ref uint pdwDataSize );

    [DllImport("WMVCore.dll", EntryPoint="WMCheckURLExtension",  SetLastError=true,
       CharSet=CharSet.Unicode, ExactSpelling=true,
       CallingConvention=CallingConvention.StdCall)] 
    private static extern int WMCheckURLExtension( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL);

    [DllImport("WMVCore.dll", EntryPoint="WMCheckURLScheme",  SetLastError=true,
       CharSet=CharSet.Unicode, ExactSpelling=true,
       CallingConvention=CallingConvention.StdCall)] 
    private static extern int WMCheckURLScheme([In, MarshalAs(UnmanagedType.LPWStr)] string pwszURLScheme);

    /// <summary>
    /// Wraps the WMWMCreateReader function
    /// </summary>
    /// <param name="Rights">Indicates the desired operation. See WMF SDK documentation</param>
    /// <returns>The reader object</returns>
    public static IWMReader CreateReader(WMT_RIGHTS Rights)
    {
      IWMReader res = null;
      Marshal.ThrowExceptionForHR(WMCreateReader(IntPtr.Zero, Rights, out res));
      return res;
    }

    /// <summary>
    /// Wraps the WMCreateSyncReader fucntion
    /// </summary>
    /// <param name="Rights">Indicates the desired operation. See WMF SDK documentation</param>
    /// <returns>The reader object</returns>
    public static IWMSyncReader CreateSyncReader(WMT_RIGHTS Rights)
    {
      IWMSyncReader  res = null;
      Marshal.ThrowExceptionForHR(WMCreateSyncReader(IntPtr.Zero, Rights, out res));
      return res;
    }

    /// <summary>
    /// Wraps the WMCreateProfileManger function
    /// </summary>
    /// <returns>The profile manager object</returns>
    public static IWMProfileManager CreateProfileManager()
    {
      IWMProfileManager res = null;
      Marshal.ThrowExceptionForHR(WMCreateProfileManager(out res));
      return res;
    }

    /// <summary>
    /// Wraps the WMCreateEditor function
    /// </summary>
    /// <returns>The meta editor object</returns>
    public static IWMMetadataEditor CreateEditor()
    {
      IWMMetadataEditor res = null;
      Marshal.ThrowExceptionForHR(WMCreateEditor(out res));
      return res;
    }
    
    /// <summary>
    /// Wraps the WMCreateIndexer function
    /// </summary>
    /// <returns>The indexer object</returns>
    public static IWMIndexer CreateIndexer()
    {
      IWMIndexer res = null;
      Marshal.ThrowExceptionForHR(WMCreateIndexer(out res));
      return res;
    }
    
    /// <summary>
    /// Wraps the WMCreateWriter function
    /// </summary>
    /// <returns>The writer object</returns>
    public static IWMWriter CreateWriter()
    {
      IWMWriter res = null;
      Marshal.ThrowExceptionForHR(WMCreateWriter(IntPtr.Zero, out res));
      return res;
    }

    /// <summary>
    /// Wraps the WMCreateWriterFileSink function
    /// </summary>
    /// <returns>The file sink object</returns>
    public static IWMWriterFileSink CreateWriterFileSink()
    {
      IWMWriterFileSink res = null;
      Marshal.ThrowExceptionForHR(WMCreateWriterFileSink(out res));
      return res;
    }
    
    /// <summary>
    /// Wraps the WMCreateWriterNetworkSink function
    /// </summary>
    /// <returns>The network sink object</returns>
    public static IWMWriterNetworkSink CreateWriterNetworkSink()
    {
      IWMWriterNetworkSink res = null;
      Marshal.ThrowExceptionForHR(WMCreateWriterNetworkSink(out res));
      return res;
    }

    /// <summary>
    /// Wraps the WMCreateWriterPushSink function
    /// </summary>
    /// <returns>The writer push sink object</returns>
    public static IWMWriterPushSink CreateWriterPushSink()
    {
      IWMWriterPushSink res = null;
      Marshal.ThrowExceptionForHR(WMCreateWriterPushSink(out res));
      return res;
    }

    /// <summary>
    /// Wraps the WMIsAvailableOffline function
    /// </summary>
    /// <param name="URL">URL to be checked</param>
    /// <param name="Language">Wide-character null-terminated string containing the 
    /// RFC 1766-compliant language ID specifying which language is desired for playback.
    /// See the WMF SDK for details.</param>
    /// <returns>True if URL can be played offline, False otherwise.</returns>
    public static bool IsAvailableOffline( string URL, string Language)
    {
      bool res = false;
      Marshal.ThrowExceptionForHR(WMIsAvailableOffline(URL, Language, out res));
      return res;
    }

    /// <summary>
    /// Wraps the WMIsContentProtected function
    /// </summary>
    /// <param name="FileName">Name of the file to check</param>
    /// <returns>True if it is protected, False otherwise.</returns>
    public static bool IsContentProtected(string FileName)
    {
      bool res = false;
      Marshal.ThrowExceptionForHR(WMIsContentProtected(FileName, out res));
      return res;
    }

    /// <summary>
    /// Wraps the WMValidateData. 
    /// Raise a CComException if data don't represent a valid ASF content
    /// </summary>
    /// <param name="Data">Buffer to check. The minimun buffer size is returned by 
    /// <see cref="Yeti.WMFSdk.WM.ValidateDataMinBuffSize"/>
    /// must be the beggining of the ASF stream</param>
    public static void ValidateData( byte[] Data)
    {
      uint DataSize = (uint)Data.Length;
      Marshal.ThrowExceptionForHR(WMValidateData(Data, ref DataSize));
    }

    /// <summary>
    /// Minimum buffer size to pass to <see cref="Yeti.WMFSdk.WM.ValidateData"/>
    /// </summary>
    public static uint ValidateDataMinBuffSize
    {
      get
      {
        uint DataSize = 0;
        Marshal.ThrowExceptionForHR(WMValidateData(null, ref DataSize));
        return DataSize;
      }
    }

    /// <summary>
    /// Wraps the WMCheckURLExtension
    /// </summary>
    /// <param name="URL">URL or file name to chekc</param>
    /// <returns>True if the specified fyle type can be opened by WMF objects. False otherwise</returns>
    public static bool CheckURLExtension( string URL)
    {
      return WMCheckURLExtension(URL) == 0; //S_OK;
    }

    /// <summary>
    /// Wraps the WMCheckURLScheme functions. Examines a network protocol 
    /// and compares it to an internal list of commonly used schemes.
    /// </summary>
    /// <param name="URLScheme">URL to check</param>
    /// <returns>True is it is a valid protocol scheme. False otherwise</returns>
    public static bool CheckURLScheme(string URLScheme)
    {
      return WMCheckURLScheme(URLScheme) == 0; 
    }
    
    private static IWMProfileManager m_ProfileManager = null;

    /// <summary>
    /// Static profile manager object. Use this property instead of calling
    /// <see cref="Yeti.WMFSdk.WM.CreateProfileManger"/> because creating and 
    /// realeasing profile managers can impact the performance.
    /// </summary>
    public static IWMProfileManager ProfileManager
    {
      get
      {
        if (m_ProfileManager == null)
        {
          m_ProfileManager = CreateProfileManager();
          IWMProfileManager2 pm2 = (IWMProfileManager2)m_ProfileManager;
          pm2.SetSystemProfileVersion(WMT_VERSION.WMT_VER_9_0);
        }
        return m_ProfileManager;
      }
    }

    //Consts.
    public const int NS_E_NO_MORE_SAMPLES = unchecked((int)0xC00D0BCF);
    public const int ASF_E_NOTFOUND = unchecked((int)0xC00D07F0);
    public const uint g_dwWMSpecialAttributes = 20;
    public const string g_wszWMDuration = "Duration";
    public const string g_wszWMBitrate = "Bitrate";
    public const string g_wszWMSeekable = "Seekable";
    public const string g_wszWMStridable = "Stridable";
    public const string g_wszWMBroadcast = "Broadcast";
    public const string g_wszWMProtected = "Is_Protected";
    public const string g_wszWMTrusted = "Is_Trusted";
    public const string g_wszWMSignature_Name = "Signature_Name";
    public const string g_wszWMHasAudio = "HasAudio";
    public const string g_wszWMHasImage = "HasImage";
    public const string g_wszWMHasScript = "HasScript";
    public const string g_wszWMHasVideo = "HasVideo";
    public const string g_wszWMCurrentBitrate = "CurrentBitrate";
    public const string g_wszWMOptimalBitrate = "OptimalBitrate";
    public const string g_wszWMHasAttachedImages = "HasAttachedImages";
    public const string g_wszWMSkipBackward = "Can_Skip_Backward";
    public const string g_wszWMSkipForward = "Can_Skip_Forward";
    public const string g_wszWMNumberOfFrames = "NumberOfFrames";
    public const string g_wszWMFileSize = "FileSize";
    public const string g_wszWMHasArbitraryDataStream = "HasArbitraryDataStream";
    public const string g_wszWMHasFileTransferStream = "HasFileTransferStream";
    public const string g_wszWMContainerFormat = "WM/ContainerFormat";
    public const uint g_dwWMContentAttributes = 5;
    public const string g_wszWMTitle = "Title";
    public const string g_wszWMAuthor = "Author";
    public const string g_wszWMDescription = "Description";
    public const string g_wszWMRating = "Rating";
    public const string g_wszWMCopyright = "Copyright";
    public const string g_wszWMUse_DRM = "Use_DRM";
    public const string g_wszWMDRM_Flags = "DRM_Flags";
    public const string g_wszWMDRM_Level = "DRM_Level";
    public const string g_wszWMUse_Advanced_DRM = "Use_Advanced_DRM";
    public const string g_wszWMDRM_KeySeed = "DRM_KeySeed";
    public const string g_wszWMDRM_KeyID = "DRM_KeyID";
    public const string g_wszWMDRM_ContentID = "DRM_ContentID";
    public const string g_wszWMDRM_IndividualizedVersion = "DRM_IndividualizedVersion";
    public const string g_wszWMDRM_LicenseAcqURL = "DRM_LicenseAcqURL";
    public const string g_wszWMDRM_V1LicenseAcqURL = "DRM_V1LicenseAcqURL";
    public const string g_wszWMDRM_HeaderSignPrivKey = "DRM_HeaderSignPrivKey";
    public const string g_wszWMDRM_LASignaturePrivKey = "DRM_LASignaturePrivKey";
    public const string g_wszWMDRM_LASignatureCert = "DRM_LASignatureCert";
    public const string g_wszWMDRM_LASignatureLicSrvCert = "DRM_LASignatureLicSrvCert";
    public const string g_wszWMDRM_LASignatureRootCert = "DRM_LASignatureRootCert";
    public const string g_wszWMAlbumTitle = "WM/AlbumTitle";
    public const string g_wszWMTrack = "WM/Track";
    public const string g_wszWMPromotionURL = "WM/PromotionURL";
    public const string g_wszWMAlbumCoverURL = "WM/AlbumCoverURL";
    public const string g_wszWMGenre = "WM/Genre";
    public const string g_wszWMYear = "WM/Year";
    public const string g_wszWMGenreID = "WM/GenreID";
    public const string g_wszWMMCDI = "WM/MCDI";
    public const string g_wszWMComposer = "WM/Composer";
    public const string g_wszWMLyrics = "WM/Lyrics";
    public const string g_wszWMTrackNumber = "WM/TrackNumber";
    public const string g_wszWMToolName = "WM/ToolName";
    public const string g_wszWMToolVersion = "WM/ToolVersion";
    public const string g_wszWMIsVBR = "IsVBR";
    public const string g_wszWMAlbumArtist = "WM/AlbumArtist";
    public const string g_wszWMBannerImageType = "BannerImageType";
    public const string g_wszWMBannerImageData = "BannerImageData";
    public const string g_wszWMBannerImageURL = "BannerImageURL";
    public const string g_wszWMCopyrightURL = "CopyrightURL";
    public const string g_wszWMAspectRatioX = "AspectRatioX";
    public const string g_wszWMAspectRatioY = "AspectRatioY";
    public const string g_wszASFLeakyBucketPairs = "ASFLeakyBucketPairs";
    public const uint g_dwWMNSCAttributes = 5;
    public const string g_wszWMNSCName = "NSC_Name";
    public const string g_wszWMNSCAddress = "NSC_Address";
    public const string g_wszWMNSCPhone = "NSC_Phone";
    public const string g_wszWMNSCEmail = "NSC_Email";
    public const string g_wszWMNSCDescription = "NSC_Description";
    public const string g_wszWMWriter = "WM/Writer";
    public const string g_wszWMConductor = "WM/Conductor";
    public const string g_wszWMProducer = "WM/Producer";
    public const string g_wszWMDirector = "WM/Director";
    public const string g_wszWMContentGroupDescription = "WM/ContentGroupDescription";
    public const string g_wszWMSubTitle = "WM/SubTitle";
    public const string g_wszWMPartOfSet = "WM/PartOfSet";
    public const string g_wszWMProtectionType = "WM/ProtectionType";
    public const string g_wszWMVideoHeight = "WM/VideoHeight";
    public const string g_wszWMVideoWidth = "WM/VideoWidth";
    public const string g_wszWMVideoFrameRate = "WM/VideoFrameRate";
    public const string g_wszWMMediaClassPrimaryID = "WM/MediaClassPrimaryID";
    public const string g_wszWMMediaClassSecondaryID = "WM/MediaClassSecondaryID";
    public const string g_wszWMPeriod = "WM/Period";
    public const string g_wszWMCategory = "WM/Category";
    public const string g_wszWMPicture = "WM/Picture";
    public const string g_wszWMLyrics_Synchronised = "WM/Lyrics_Synchronised";
    public const string g_wszWMOriginalLyricist = "WM/OriginalLyricist";
    public const string g_wszWMOriginalArtist = "WM/OriginalArtist";
    public const string g_wszWMOriginalAlbumTitle = "WM/OriginalAlbumTitle";
    public const string g_wszWMOriginalReleaseYear = "WM/OriginalReleaseYear";
    public const string g_wszWMOriginalFilename = "WM/OriginalFilename";
    public const string g_wszWMPublisher = "WM/Publisher";
    public const string g_wszWMEncodedBy = "WM/EncodedBy";
    public const string g_wszWMEncodingSettings = "WM/EncodingSettings";
    public const string g_wszWMEncodingTime = "WM/EncodingTime";
    public const string g_wszWMAuthorURL = "WM/AuthorURL";
    public const string g_wszWMUserWebURL = "WM/UserWebURL";
    public const string g_wszWMAudioFileURL = "WM/AudioFileURL";
    public const string g_wszWMAudioSourceURL = "WM/AudioSourceURL";
    public const string g_wszWMLanguage = "WM/Language";
    public const string g_wszWMParentalRating = "WM/ParentalRating";
    public const string g_wszWMBeatsPerMinute = "WM/BeatsPerMinute";
    public const string g_wszWMInitialKey = "WM/InitialKey";
    public const string g_wszWMMood = "WM/Mood";
    public const string g_wszWMText = "WM/Text";
    public const string g_wszWMDVDID = "WM/DVDID";
    public const string g_wszWMWMContentID = "WM/WMContentID";
    public const string g_wszWMWMCollectionID = "WM/WMCollectionID";
    public const string g_wszWMWMCollectionGroupID = "WM/WMCollectionGroupID";
    public const string g_wszWMUniqueFileIdentifier = "WM/UniqueFileIdentifier";
    public const string g_wszWMModifiedBy = "WM/ModifiedBy";
    public const string g_wszWMRadioStationName = "WM/RadioStationName";
    public const string g_wszWMRadioStationOwner = "WM/RadioStationOwner";
    public const string g_wszWMPlaylistDelay = "WM/PlaylistDelay";
    public const string g_wszWMCodec = "WM/Codec";
    public const string g_wszWMDRM = "WM/DRM";
    public const string g_wszWMISRC = "WM/ISRC";
    public const string g_wszWMProvider = "WM/Provider";
    public const string g_wszWMProviderRating = "WM/ProviderRating";
    public const string g_wszWMProviderStyle = "WM/ProviderStyle";
    public const string g_wszWMContentDistributor = "WM/ContentDistributor";
    public const string g_wszWMSubscriptionContentID = "WM/SubscriptionContentID";
    public const string g_wszWMWMADRCPeakReference = "WM/WMADRCPeakReference";
    public const string g_wszWMWMADRCPeakTarget = "WM/WMADRCPeakTarget";
    public const string g_wszWMWMADRCAverageReference = "WM/WMADRCAverageReference";
    public const string g_wszWMWMADRCAverageTarget = "WM/WMADRCAverageTarget";
    public const string g_wszEarlyDataDelivery = "EarlyDataDelivery";
    public const string g_wszJustInTimeDecode = "JustInTimeDecode";
    public const string g_wszSingleOutputBuffer = "SingleOutputBuffer";
    public const string g_wszSoftwareScaling = "SoftwareScaling";
    public const string g_wszDeliverOnReceive = "DeliverOnReceive";
    public const string g_wszScrambledAudio = "ScrambledAudio";
    public const string g_wszDedicatedDeliveryThread = "DedicatedDeliveryThread";
    public const string g_wszEnableDiscreteOutput = "EnableDiscreteOutput";
    public const string g_wszSpeakerConfig = "SpeakerConfig";
    public const string g_wszDynamicRangeControl = "DynamicRangeControl";
    public const string g_wszAllowInterlacedOutput = "AllowInterlacedOutput";
    public const string g_wszVideoSampleDurations = "VideoSampleDurations";
    public const string g_wszStreamLanguage = "StreamLanguage";
    public const string g_wszDeinterlaceMode = "DeinterlaceMode";
    public const string g_wszInitialPatternForInverseTelecine = "InitialPatternForInverseTelecine";
    public const string g_wszJPEGCompressionQuality = "JPEGCompressionQuality";
    public const string g_wszWatermarkCLSID = "WatermarkCLSID";
    public const string g_wszWatermarkConfig = "WatermarkConfig";
    public const string g_wszInterlacedCoding = "InterlacedCoding";
    public const string g_wszFixedFrameRate = "FixedFrameRate";
    public const string g_wszOriginalSourceFormatTag = "_SOURCEFORMATTAG";
    public const string g_wszOriginalWaveFormat = "_ORIGINALWAVEFORMAT";
    public const string g_wszEDL = "_EDL";
    public const string g_wszComplexity = "_COMPLEXITYEX";
    public const string g_wszDecoderComplexityRequested = "_DECODERCOMPLEXITYPROFILE";
    public const string g_wszReloadIndexOnSeek = "ReloadIndexOnSeek";
    public const string g_wszStreamNumIndexObjects = "StreamNumIndexObjects";
    public const string g_wszFailSeekOnError = "FailSeekOnError";
    public const string g_wszPermitSeeksBeyondEndOfStream = "PermitSeeksBeyondEndOfStream";
    public const string g_wszUsePacketAtSeekPoint = "UsePacketAtSeekPoint";
    public const string g_wszSourceBufferTime = "SourceBufferTime";
    public const string g_wszSourceMaxBytesAtOnce = "SourceMaxBytesAtOnce";
    public const string g_wszVBREnabled = "_VBRENABLED";
    public const string g_wszVBRQuality = "_VBRQUALITY";
    public const string g_wszVBRBitrateMax = "_RMAX";
    public const string g_wszVBRBufferWindowMax = "_BMAX";
    public const string g_wszVBRPeak = "VBR Peak";
    public const string g_wszBufferAverage = "Buffer Average";
    public const string g_wszComplexityMax = "_COMPLEXITYEXMAX";
    public const string g_wszComplexityOffline = "_COMPLEXITYEXOFFLINE";
    public const string g_wszComplexityLive = "_COMPLEXITYEXLIVE";
    public const string g_wszIsVBRSupported = "_ISVBRSUPPORTED";
    public const string g_wszNumPasses = "_PASSESUSED";
    public const string g_wszMusicSpeechClassMode = "MusicSpeechClassMode";
    public const string g_wszMusicClassMode = "MusicClassMode";
    public const string g_wszSpeechClassMode = "SpeechClassMode";
    public const string g_wszMixedClassMode = "MixedClassMode";
    public const string g_wszSpeechCaps = "SpeechFormatCap";
    public const string g_wszPeakValue = "PeakValue";
    public const string g_wszAverageLevel = "AverageLevel";
    public const string g_wszFold6To2Channels3 = "Fold6To2Channels3";
    public const string g_wszFoldToChannelsTemplate = "Fold%luTo%luChannels%lu";
    public const string g_wszDeviceConformanceTemplate = "DeviceConformanceTemplate";
    public const string g_wszEnableFrameInterpolation = "EnableFrameInterpolation";

	}
}
