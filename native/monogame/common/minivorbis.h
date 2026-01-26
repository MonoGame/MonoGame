/*
  minivorbis.h -- libvorbis decoder in a single header
  Project URL: https://github.com/edubart/minivorbis

  This is libogg 1.3.4 + libvorbis 1.3.7 contained in a single header
  to be bundled in C/C++ applications with ease for decoding OGG sound files.
  Ogg Vorbis is a open general-purpose compressed audio format
  for mid to high quality audio and music at fixed and variable bitrates.

  Do the following in *one* C file to implement Ogg and Vorbis:
    #define OGG_IMPL
    #define VORBIS_IMPL

  Optionally provide the following defines:
    OV_EXCLUDE_STATIC_CALLBACKS     - exclude the default static callbacks

  Note that almost no modification was made in the Ogg/Vorbis implementation code,
  thus there are some C variable names that may collide with your code,
  therefore it is best to declare the implementation in dedicated C file.

  LICENSE
    BSD-like License, same as libogg and libvorbis, see end of file.
*/
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2002             *
 * by the Xiph.Org Foundation http://www.xiph.org/                  *
 *                                                                  *
 ********************************************************************

 function: Define a consistent set of types on each platform.

 ********************************************************************/
#ifndef _OS_TYPES_H
#define _OS_TYPES_H

/* make it easy on the folks that want to compile the libs with a
   different malloc than stdlib */
#define _ogg_malloc  malloc
#define _ogg_calloc  calloc
#define _ogg_realloc realloc
#define _ogg_free    free

#if defined(_WIN32)

#  if defined(__CYGWIN__)
#    include <stdint.h>
     typedef int16_t ogg_int16_t;
     typedef uint16_t ogg_uint16_t;
     typedef int32_t ogg_int32_t;
     typedef uint32_t ogg_uint32_t;
     typedef int64_t ogg_int64_t;
     typedef uint64_t ogg_uint64_t;
#  elif defined(__MINGW32__)
#    include <sys/types.h>
     typedef short ogg_int16_t;
     typedef unsigned short ogg_uint16_t;
     typedef int ogg_int32_t;
     typedef unsigned int ogg_uint32_t;
     typedef long long ogg_int64_t;
     typedef unsigned long long ogg_uint64_t;
#  elif defined(__MWERKS__)
     typedef long long ogg_int64_t;
     typedef unsigned long long ogg_uint64_t;
     typedef int ogg_int32_t;
     typedef unsigned int ogg_uint32_t;
     typedef short ogg_int16_t;
     typedef unsigned short ogg_uint16_t;
#  else
#    if defined(_MSC_VER) && (_MSC_VER >= 1800) /* MSVC 2013 and newer */
#      include <stdint.h>
       typedef int16_t ogg_int16_t;
       typedef uint16_t ogg_uint16_t;
       typedef int32_t ogg_int32_t;
       typedef uint32_t ogg_uint32_t;
       typedef int64_t ogg_int64_t;
       typedef uint64_t ogg_uint64_t;
#    else
       /* MSVC/Borland */
       typedef __int64 ogg_int64_t;
       typedef __int32 ogg_int32_t;
       typedef unsigned __int32 ogg_uint32_t;
       typedef unsigned __int64 ogg_uint64_t;
       typedef __int16 ogg_int16_t;
       typedef unsigned __int16 ogg_uint16_t;
#    endif
#  endif

#elif (defined(__APPLE__) && defined(__MACH__)) /* MacOS X Framework build */

#  include <sys/types.h>
   typedef int16_t ogg_int16_t;
   typedef uint16_t ogg_uint16_t;
   typedef int32_t ogg_int32_t;
   typedef uint32_t ogg_uint32_t;
   typedef int64_t ogg_int64_t;
   typedef uint64_t ogg_uint64_t;

#elif defined(__HAIKU__)

  /* Haiku */
#  include <sys/types.h>
   typedef short ogg_int16_t;
   typedef unsigned short ogg_uint16_t;
   typedef int ogg_int32_t;
   typedef unsigned int ogg_uint32_t;
   typedef long long ogg_int64_t;
   typedef unsigned long long ogg_uint64_t;

#elif defined(__BEOS__)

   /* Be */
#  include <inttypes.h>
   typedef int16_t ogg_int16_t;
   typedef uint16_t ogg_uint16_t;
   typedef int32_t ogg_int32_t;
   typedef uint32_t ogg_uint32_t;
   typedef int64_t ogg_int64_t;
   typedef uint64_t ogg_uint64_t;

#elif defined (__EMX__)

   /* OS/2 GCC */
   typedef short ogg_int16_t;
   typedef unsigned short ogg_uint16_t;
   typedef int ogg_int32_t;
   typedef unsigned int ogg_uint32_t;
   typedef long long ogg_int64_t;
   typedef unsigned long long ogg_uint64_t;


#elif defined (DJGPP)

   /* DJGPP */
   typedef short ogg_int16_t;
   typedef int ogg_int32_t;
   typedef unsigned int ogg_uint32_t;
   typedef long long ogg_int64_t;
   typedef unsigned long long ogg_uint64_t;

#elif defined(R5900)

   /* PS2 EE */
   typedef long ogg_int64_t;
   typedef unsigned long ogg_uint64_t;
   typedef int ogg_int32_t;
   typedef unsigned ogg_uint32_t;
   typedef short ogg_int16_t;

#elif defined(__SYMBIAN32__)

   /* Symbian GCC */
   typedef signed short ogg_int16_t;
   typedef unsigned short ogg_uint16_t;
   typedef signed int ogg_int32_t;
   typedef unsigned int ogg_uint32_t;
   typedef long long int ogg_int64_t;
   typedef unsigned long long int ogg_uint64_t;

#elif defined(__TMS320C6X__)

   /* TI C64x compiler */
   typedef signed short ogg_int16_t;
   typedef unsigned short ogg_uint16_t;
   typedef signed int ogg_int32_t;
   typedef unsigned int ogg_uint32_t;
   typedef long long int ogg_int64_t;
   typedef unsigned long long int ogg_uint64_t;

#else

/* config_types.h */
#include <stdint.h>
typedef int16_t ogg_int16_t;
typedef uint16_t ogg_uint16_t;
typedef int32_t ogg_int32_t;
typedef uint32_t ogg_uint32_t;
typedef int64_t ogg_int64_t;
typedef uint64_t ogg_uint64_t;

#endif

#endif  /* _OS_TYPES_H */
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2007             *
 * by the Xiph.Org Foundation http://www.xiph.org/                  *
 *                                                                  *
 ********************************************************************

 function: toplevel libogg include

 ********************************************************************/
#ifndef _OGG_H
#define _OGG_H

#ifdef __cplusplus
extern "C" {
#endif

#include <stddef.h>
/*#include <ogg/os_types.h>*/

typedef struct {
  void *iov_base;
  size_t iov_len;
} ogg_iovec_t;

typedef struct {
  long endbyte;
  int  endbit;

  unsigned char *buffer;
  unsigned char *ptr;
  long storage;
} oggpack_buffer;

/* ogg_page is used to encapsulate the data in one Ogg bitstream page *****/

typedef struct {
  unsigned char *header;
  long header_len;
  unsigned char *body;
  long body_len;
} ogg_page;

/* ogg_stream_state contains the current encode/decode state of a logical
   Ogg bitstream **********************************************************/

typedef struct {
  unsigned char   *body_data;    /* bytes from packet bodies */
  long    body_storage;          /* storage elements allocated */
  long    body_fill;             /* elements stored; fill mark */
  long    body_returned;         /* elements of fill returned */


  int     *lacing_vals;      /* The values that will go to the segment table */
  ogg_int64_t *granule_vals; /* granulepos values for headers. Not compact
                                this way, but it is simple coupled to the
                                lacing fifo */
  long    lacing_storage;
  long    lacing_fill;
  long    lacing_packet;
  long    lacing_returned;

  unsigned char    header[282];      /* working space for header encode */
  int              header_fill;

  int     e_o_s;          /* set when we have buffered the last packet in the
                             logical bitstream */
  int     b_o_s;          /* set after we've written the initial page
                             of a logical bitstream */
  long    serialno;
  long    pageno;
  ogg_int64_t  packetno;  /* sequence number for decode; the framing
                             knows where there's a hole in the data,
                             but we need coupling so that the codec
                             (which is in a separate abstraction
                             layer) also knows about the gap */
  ogg_int64_t   granulepos;

} ogg_stream_state;

/* ogg_packet is used to encapsulate the data and metadata belonging
   to a single raw Ogg/Vorbis packet *************************************/

typedef struct {
  unsigned char *packet;
  long  bytes;
  long  b_o_s;
  long  e_o_s;

  ogg_int64_t  granulepos;

  ogg_int64_t  packetno;     /* sequence number for decode; the framing
                                knows where there's a hole in the data,
                                but we need coupling so that the codec
                                (which is in a separate abstraction
                                layer) also knows about the gap */
} ogg_packet;

typedef struct {
  unsigned char *data;
  int storage;
  int fill;
  int returned;

  int unsynced;
  int headerbytes;
  int bodybytes;
} ogg_sync_state;

/* Ogg BITSTREAM PRIMITIVES: bitstream ************************/

extern void  oggpack_writeinit(oggpack_buffer *b);
extern int   oggpack_writecheck(oggpack_buffer *b);
extern void  oggpack_writetrunc(oggpack_buffer *b,long bits);
extern void  oggpack_writealign(oggpack_buffer *b);
extern void  oggpack_writecopy(oggpack_buffer *b,void *source,long bits);
extern void  oggpack_reset(oggpack_buffer *b);
extern void  oggpack_writeclear(oggpack_buffer *b);
extern void  oggpack_readinit(oggpack_buffer *b,unsigned char *buf,int bytes);
extern void  oggpack_write(oggpack_buffer *b,unsigned long value,int bits);
extern long  oggpack_look(oggpack_buffer *b,int bits);
extern long  oggpack_look1(oggpack_buffer *b);
extern void  oggpack_adv(oggpack_buffer *b,int bits);
extern void  oggpack_adv1(oggpack_buffer *b);
extern long  oggpack_read(oggpack_buffer *b,int bits);
extern long  oggpack_read1(oggpack_buffer *b);
extern long  oggpack_bytes(oggpack_buffer *b);
extern long  oggpack_bits(oggpack_buffer *b);
extern unsigned char *oggpack_get_buffer(oggpack_buffer *b);

extern void  oggpackB_writeinit(oggpack_buffer *b);
extern int   oggpackB_writecheck(oggpack_buffer *b);
extern void  oggpackB_writetrunc(oggpack_buffer *b,long bits);
extern void  oggpackB_writealign(oggpack_buffer *b);
extern void  oggpackB_writecopy(oggpack_buffer *b,void *source,long bits);
extern void  oggpackB_reset(oggpack_buffer *b);
extern void  oggpackB_writeclear(oggpack_buffer *b);
extern void  oggpackB_readinit(oggpack_buffer *b,unsigned char *buf,int bytes);
extern void  oggpackB_write(oggpack_buffer *b,unsigned long value,int bits);
extern long  oggpackB_look(oggpack_buffer *b,int bits);
extern long  oggpackB_look1(oggpack_buffer *b);
extern void  oggpackB_adv(oggpack_buffer *b,int bits);
extern void  oggpackB_adv1(oggpack_buffer *b);
extern long  oggpackB_read(oggpack_buffer *b,int bits);
extern long  oggpackB_read1(oggpack_buffer *b);
extern long  oggpackB_bytes(oggpack_buffer *b);
extern long  oggpackB_bits(oggpack_buffer *b);
extern unsigned char *oggpackB_get_buffer(oggpack_buffer *b);

/* Ogg BITSTREAM PRIMITIVES: encoding **************************/

extern int      ogg_stream_packetin(ogg_stream_state *os, ogg_packet *op);
extern int      ogg_stream_iovecin(ogg_stream_state *os, ogg_iovec_t *iov,
                                   int count, long e_o_s, ogg_int64_t granulepos);
extern int      ogg_stream_pageout(ogg_stream_state *os, ogg_page *og);
extern int      ogg_stream_pageout_fill(ogg_stream_state *os, ogg_page *og, int nfill);
extern int      ogg_stream_flush(ogg_stream_state *os, ogg_page *og);
extern int      ogg_stream_flush_fill(ogg_stream_state *os, ogg_page *og, int nfill);

/* Ogg BITSTREAM PRIMITIVES: decoding **************************/

extern int      ogg_sync_init(ogg_sync_state *oy);
extern int      ogg_sync_clear(ogg_sync_state *oy);
extern int      ogg_sync_reset(ogg_sync_state *oy);
extern int      ogg_sync_destroy(ogg_sync_state *oy);
extern int      ogg_sync_check(ogg_sync_state *oy);

extern char    *ogg_sync_buffer(ogg_sync_state *oy, long size);
extern int      ogg_sync_wrote(ogg_sync_state *oy, long bytes);
extern long     ogg_sync_pageseek(ogg_sync_state *oy,ogg_page *og);
extern int      ogg_sync_pageout(ogg_sync_state *oy, ogg_page *og);
extern int      ogg_stream_pagein(ogg_stream_state *os, ogg_page *og);
extern int      ogg_stream_packetout(ogg_stream_state *os,ogg_packet *op);
extern int      ogg_stream_packetpeek(ogg_stream_state *os,ogg_packet *op);

/* Ogg BITSTREAM PRIMITIVES: general ***************************/

extern int      ogg_stream_init(ogg_stream_state *os,int serialno);
extern int      ogg_stream_clear(ogg_stream_state *os);
extern int      ogg_stream_reset(ogg_stream_state *os);
extern int      ogg_stream_reset_serialno(ogg_stream_state *os,int serialno);
extern int      ogg_stream_destroy(ogg_stream_state *os);
extern int      ogg_stream_check(ogg_stream_state *os);
extern int      ogg_stream_eos(ogg_stream_state *os);

extern void     ogg_page_checksum_set(ogg_page *og);

extern int      ogg_page_version(const ogg_page *og);
extern int      ogg_page_continued(const ogg_page *og);
extern int      ogg_page_bos(const ogg_page *og);
extern int      ogg_page_eos(const ogg_page *og);
extern ogg_int64_t  ogg_page_granulepos(const ogg_page *og);
extern int      ogg_page_serialno(const ogg_page *og);
extern long     ogg_page_pageno(const ogg_page *og);
extern int      ogg_page_packets(const ogg_page *og);

extern void     ogg_packet_clear(ogg_packet *op);


#ifdef __cplusplus
}
#endif

#endif  /* _OGG_H */
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2001             *
 * by the Xiph.Org Foundation https://xiph.org/                     *

 ********************************************************************

 function: libvorbis codec headers

 ********************************************************************/

#ifndef _vorbis_codec_h_
#define _vorbis_codec_h_

#ifdef __cplusplus
extern "C"
{
#endif /* __cplusplus */

/*#include <ogg/ogg.h>*/

typedef struct vorbis_info{
  int version;
  int channels;
  long rate;

  /* The below bitrate declarations are *hints*.
     Combinations of the three values carry the following implications:

     all three set to the same value:
       implies a fixed rate bitstream
     only nominal set:
       implies a VBR stream that averages the nominal bitrate.  No hard
       upper/lower limit
     upper and or lower set:
       implies a VBR bitstream that obeys the bitrate limits. nominal
       may also be set to give a nominal rate.
     none set:
       the coder does not care to speculate.
  */

  long bitrate_upper;
  long bitrate_nominal;
  long bitrate_lower;
  long bitrate_window;

  void *codec_setup;
} vorbis_info;

/* vorbis_dsp_state buffers the current vorbis audio
   analysis/synthesis state.  The DSP state belongs to a specific
   logical bitstream ****************************************************/
typedef struct vorbis_dsp_state{
  int analysisp;
  vorbis_info *vi;

  float **pcm;
  float **pcmret;
  int      pcm_storage;
  int      pcm_current;
  int      pcm_returned;

  int  preextrapolate;
  int  eofflag;

  long lW;
  long W;
  long nW;
  long centerW;

  ogg_int64_t granulepos;
  ogg_int64_t sequence;

  ogg_int64_t glue_bits;
  ogg_int64_t time_bits;
  ogg_int64_t floor_bits;
  ogg_int64_t res_bits;

  void       *backend_state;
} vorbis_dsp_state;

typedef struct vorbis_block{
  /* necessary stream state for linking to the framing abstraction */
  float  **pcm;       /* this is a pointer into local storage */
  oggpack_buffer opb;

  long  lW;
  long  W;
  long  nW;
  int   pcmend;
  int   mode;

  int         eofflag;
  ogg_int64_t granulepos;
  ogg_int64_t sequence;
  vorbis_dsp_state *vd; /* For read-only access of configuration */

  /* local storage to avoid remallocing; it's up to the mapping to
     structure it */
  void               *localstore;
  long                localtop;
  long                localalloc;
  long                totaluse;
  struct alloc_chain *reap;

  /* bitmetrics for the frame */
  long glue_bits;
  long time_bits;
  long floor_bits;
  long res_bits;

  void *internal;

} vorbis_block;

/* vorbis_block is a single block of data to be processed as part of
the analysis/synthesis stream; it belongs to a specific logical
bitstream, but is independent from other vorbis_blocks belonging to
that logical bitstream. *************************************************/

struct alloc_chain{
  void *ptr;
  struct alloc_chain *next;
};

/* vorbis_info contains all the setup information specific to the
   specific compression/decompression mode in progress (eg,
   psychoacoustic settings, channel setup, options, codebook
   etc). vorbis_info and substructures are in backends.h.
*********************************************************************/

/* the comments are not part of vorbis_info so that vorbis_info can be
   static storage */
typedef struct vorbis_comment{
  /* unlimited user comment fields.  libvorbis writes 'libvorbis'
     whatever vendor is set to in encode */
  char **user_comments;
  int   *comment_lengths;
  int    comments;
  char  *vendor;

} vorbis_comment;


/* libvorbis encodes in two abstraction layers; first we perform DSP
   and produce a packet (see docs/analysis.txt).  The packet is then
   coded into a framed OggSquish bitstream by the second layer (see
   docs/framing.txt).  Decode is the reverse process; we sync/frame
   the bitstream and extract individual packets, then decode the
   packet back into PCM audio.

   The extra framing/packetizing is used in streaming formats, such as
   files.  Over the net (such as with UDP), the framing and
   packetization aren't necessary as they're provided by the transport
   and the streaming layer is not used */

/* Vorbis PRIMITIVES: general ***************************************/

extern void     vorbis_info_init(vorbis_info *vi);
extern void     vorbis_info_clear(vorbis_info *vi);
extern int      vorbis_info_blocksize(vorbis_info *vi,int zo);
extern void     vorbis_comment_init(vorbis_comment *vc);
extern void     vorbis_comment_add(vorbis_comment *vc, const char *comment);
extern void     vorbis_comment_add_tag(vorbis_comment *vc,
                                       const char *tag, const char *contents);
extern char    *vorbis_comment_query(vorbis_comment *vc, const char *tag, int count);
extern int      vorbis_comment_query_count(vorbis_comment *vc, const char *tag);
extern void     vorbis_comment_clear(vorbis_comment *vc);

extern int      vorbis_block_init(vorbis_dsp_state *v, vorbis_block *vb);
extern int      vorbis_block_clear(vorbis_block *vb);
extern void     vorbis_dsp_clear(vorbis_dsp_state *v);
extern double   vorbis_granule_time(vorbis_dsp_state *v,
                                    ogg_int64_t granulepos);

extern const char *vorbis_version_string(void);

/* Vorbis PRIMITIVES: analysis/DSP layer ****************************/

extern int      vorbis_analysis_init(vorbis_dsp_state *v,vorbis_info *vi);
extern int      vorbis_commentheader_out(vorbis_comment *vc, ogg_packet *op);
extern int      vorbis_analysis_headerout(vorbis_dsp_state *v,
                                          vorbis_comment *vc,
                                          ogg_packet *op,
                                          ogg_packet *op_comm,
                                          ogg_packet *op_code);
extern float  **vorbis_analysis_buffer(vorbis_dsp_state *v,int vals);
extern int      vorbis_analysis_wrote(vorbis_dsp_state *v,int vals);
extern int      vorbis_analysis_blockout(vorbis_dsp_state *v,vorbis_block *vb);
extern int      vorbis_analysis(vorbis_block *vb,ogg_packet *op);

extern int      vorbis_bitrate_addblock(vorbis_block *vb);
extern int      vorbis_bitrate_flushpacket(vorbis_dsp_state *vd,
                                           ogg_packet *op);

/* Vorbis PRIMITIVES: synthesis layer *******************************/
extern int      vorbis_synthesis_idheader(ogg_packet *op);
extern int      vorbis_synthesis_headerin(vorbis_info *vi,vorbis_comment *vc,
                                          ogg_packet *op);

extern int      vorbis_synthesis_init(vorbis_dsp_state *v,vorbis_info *vi);
extern int      vorbis_synthesis_restart(vorbis_dsp_state *v);
extern int      vorbis_synthesis(vorbis_block *vb,ogg_packet *op);
extern int      vorbis_synthesis_trackonly(vorbis_block *vb,ogg_packet *op);
extern int      vorbis_synthesis_blockin(vorbis_dsp_state *v,vorbis_block *vb);
extern int      vorbis_synthesis_pcmout(vorbis_dsp_state *v,float ***pcm);
extern int      vorbis_synthesis_lapout(vorbis_dsp_state *v,float ***pcm);
extern int      vorbis_synthesis_read(vorbis_dsp_state *v,int samples);
extern long     vorbis_packet_blocksize(vorbis_info *vi,ogg_packet *op);

extern int      vorbis_synthesis_halfrate(vorbis_info *v,int flag);
extern int      vorbis_synthesis_halfrate_p(vorbis_info *v);

/* Vorbis ERRORS and return codes ***********************************/

#define OV_FALSE      -1
#define OV_EOF        -2
#define OV_HOLE       -3

#define OV_EREAD      -128
#define OV_EFAULT     -129
#define OV_EIMPL      -130
#define OV_EINVAL     -131
#define OV_ENOTVORBIS -132
#define OV_EBADHEADER -133
#define OV_EVERSION   -134
#define OV_ENOTAUDIO  -135
#define OV_EBADPACKET -136
#define OV_EBADLINK   -137
#define OV_ENOSEEK    -138

#ifdef __cplusplus
}
#endif /* __cplusplus */

#endif

/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2007             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: stdio-based convenience library for opening/seeking/decoding

 ********************************************************************/

#ifndef _OV_FILE_H_
#define _OV_FILE_H_

#ifdef __cplusplus
extern "C"
{
#endif /* __cplusplus */

#include <stdio.h>
/*#include "codec.h"*/

/* The function prototypes for the callbacks are basically the same as for
 * the stdio functions fread, fseek, fclose, ftell.
 * The one difference is that the FILE * arguments have been replaced with
 * a void * - this is to be used as a pointer to whatever internal data these
 * functions might need. In the stdio case, it's just a FILE * cast to a void *
 *
 * If you use other functions, check the docs for these functions and return
 * the right values. For seek_func(), you *MUST* return -1 if the stream is
 * unseekable
 */
typedef struct {
  size_t (*read_func)  (void *ptr, size_t size, size_t nmemb, void *datasource);
  int    (*seek_func)  (void *datasource, ogg_int64_t offset, int whence);
  int    (*close_func) (void *datasource);
  long   (*tell_func)  (void *datasource);
} ov_callbacks;

#ifndef OV_EXCLUDE_STATIC_CALLBACKS

/* a few sets of convenient callbacks, especially for use under
 * Windows where ov_open_callbacks() should always be used instead of
 * ov_open() to avoid problems with incompatible crt.o version linking
 * issues. */

static int _ov_header_fseek_wrap(FILE *f,ogg_int64_t off,int whence){
  if(f==NULL)return(-1);

#ifdef __MINGW32__
  return fseeko64(f,off,whence);
#elif defined (_WIN32)
  return _fseeki64(f,off,whence);
#else
  return fseek(f,off,whence);
#endif
}

/* These structs below (OV_CALLBACKS_DEFAULT etc) are defined here as
 * static data. That means that every file which includes this header
 * will get its own copy of these structs whether it uses them or
 * not unless it #defines OV_EXCLUDE_STATIC_CALLBACKS.
 * These static symbols are essential on platforms such as Windows on
 * which several different versions of stdio support may be linked to
 * by different DLLs, and we need to be certain we know which one
 * we're using (the same one as the main application).
 */

static ov_callbacks OV_CALLBACKS_DEFAULT = {
  (size_t (*)(void *, size_t, size_t, void *))  fread,
  (int (*)(void *, ogg_int64_t, int))           _ov_header_fseek_wrap,
  (int (*)(void *))                             fclose,
  (long (*)(void *))                            ftell
};

static ov_callbacks OV_CALLBACKS_NOCLOSE = {
  (size_t (*)(void *, size_t, size_t, void *))  fread,
  (int (*)(void *, ogg_int64_t, int))           _ov_header_fseek_wrap,
  (int (*)(void *))                             NULL,
  (long (*)(void *))                            ftell
};

static ov_callbacks OV_CALLBACKS_STREAMONLY = {
  (size_t (*)(void *, size_t, size_t, void *))  fread,
  (int (*)(void *, ogg_int64_t, int))           NULL,
  (int (*)(void *))                             fclose,
  (long (*)(void *))                            NULL
};

static ov_callbacks OV_CALLBACKS_STREAMONLY_NOCLOSE = {
  (size_t (*)(void *, size_t, size_t, void *))  fread,
  (int (*)(void *, ogg_int64_t, int))           NULL,
  (int (*)(void *))                             NULL,
  (long (*)(void *))                            NULL
};

#endif

#define  NOTOPEN   0
#define  PARTOPEN  1
#define  OPENED    2
#define  STREAMSET 3
#define  INITSET   4

typedef struct OggVorbis_File {
  void            *datasource; /* Pointer to a FILE *, etc. */
  int              seekable;
  ogg_int64_t      offset;
  ogg_int64_t      end;
  ogg_sync_state   oy;

  /* If the FILE handle isn't seekable (eg, a pipe), only the current
     stream appears */
  int              links;
  ogg_int64_t     *offsets;
  ogg_int64_t     *dataoffsets;
  long            *serialnos;
  ogg_int64_t     *pcmlengths; /* overloaded to maintain binary
                                  compatibility; x2 size, stores both
                                  beginning and end values */
  vorbis_info     *vi;
  vorbis_comment  *vc;

  /* Decoding working state local storage */
  ogg_int64_t      pcm_offset;
  int              ready_state;
  long             current_serialno;
  int              current_link;

  double           bittrack;
  double           samptrack;

  ogg_stream_state os; /* take physical pages, weld into a logical
                          stream of packets */
  vorbis_dsp_state vd; /* central working state for the packet->PCM decoder */
  vorbis_block     vb; /* local working space for packet->PCM decode */

  ov_callbacks callbacks;

} OggVorbis_File;


extern int ov_clear(OggVorbis_File *vf);
extern int ov_fopen(const char *path,OggVorbis_File *vf);
extern int ov_open(FILE *f,OggVorbis_File *vf,const char *initial,long ibytes);
extern int ov_open_callbacks(void *datasource, OggVorbis_File *vf,
                const char *initial, long ibytes, ov_callbacks callbacks);

extern int ov_test(FILE *f,OggVorbis_File *vf,const char *initial,long ibytes);
extern int ov_test_callbacks(void *datasource, OggVorbis_File *vf,
                const char *initial, long ibytes, ov_callbacks callbacks);
extern int ov_test_open(OggVorbis_File *vf);

extern long ov_bitrate(OggVorbis_File *vf,int i);
extern long ov_bitrate_instant(OggVorbis_File *vf);
extern long ov_streams(OggVorbis_File *vf);
extern long ov_seekable(OggVorbis_File *vf);
extern long ov_serialnumber(OggVorbis_File *vf,int i);

extern ogg_int64_t ov_raw_total(OggVorbis_File *vf,int i);
extern ogg_int64_t ov_pcm_total(OggVorbis_File *vf,int i);
extern double ov_time_total(OggVorbis_File *vf,int i);

extern int ov_raw_seek(OggVorbis_File *vf,ogg_int64_t pos);
extern int ov_pcm_seek(OggVorbis_File *vf,ogg_int64_t pos);
extern int ov_pcm_seek_page(OggVorbis_File *vf,ogg_int64_t pos);
extern int ov_time_seek(OggVorbis_File *vf,double pos);
extern int ov_time_seek_page(OggVorbis_File *vf,double pos);

extern int ov_raw_seek_lap(OggVorbis_File *vf,ogg_int64_t pos);
extern int ov_pcm_seek_lap(OggVorbis_File *vf,ogg_int64_t pos);
extern int ov_pcm_seek_page_lap(OggVorbis_File *vf,ogg_int64_t pos);
extern int ov_time_seek_lap(OggVorbis_File *vf,double pos);
extern int ov_time_seek_page_lap(OggVorbis_File *vf,double pos);

extern ogg_int64_t ov_raw_tell(OggVorbis_File *vf);
extern ogg_int64_t ov_pcm_tell(OggVorbis_File *vf);
extern double ov_time_tell(OggVorbis_File *vf);

extern vorbis_info *ov_info(OggVorbis_File *vf,int link);
extern vorbis_comment *ov_comment(OggVorbis_File *vf,int link);

extern long ov_read_float(OggVorbis_File *vf,float ***pcm_channels,int samples,
                          int *bitstream);
extern long ov_read_filter(OggVorbis_File *vf,char *buffer,int length,
                          int bigendianp,int word,int sgned,int *bitstream,
                          void (*filter)(float **pcm,long channels,long samples,void *filter_param),void *filter_param);
extern long ov_read(OggVorbis_File *vf,char *buffer,int length,
                    int bigendianp,int word,int sgned,int *bitstream);
extern int ov_crosslap(OggVorbis_File *vf1,OggVorbis_File *vf2);

extern int ov_halfrate(OggVorbis_File *vf,int flag);
extern int ov_halfrate_p(OggVorbis_File *vf);

#ifdef __cplusplus
}
#endif /* __cplusplus */

#endif

#ifdef OGG_IMPL
#ifdef __cplusplus
extern "C" {
#endif
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE Ogg CONTAINER SOURCE CODE.              *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2014             *
 * by the Xiph.Org Foundation http://www.xiph.org/                  *
 *                                                                  *
 ********************************************************************

  function: packing variable sized words into an octet stream

 ********************************************************************/

/* We're 'LSb' endian; if we write a word but read individual bits,
   then we'll read the lsb first */

#include <string.h>
#include <stdlib.h>
#include <limits.h>
/*#include <ogg/ogg.h>*/

#define BUFFER_INCREMENT 256

static const unsigned long mask[]=
{0x00000000,0x00000001,0x00000003,0x00000007,0x0000000f,
 0x0000001f,0x0000003f,0x0000007f,0x000000ff,0x000001ff,
 0x000003ff,0x000007ff,0x00000fff,0x00001fff,0x00003fff,
 0x00007fff,0x0000ffff,0x0001ffff,0x0003ffff,0x0007ffff,
 0x000fffff,0x001fffff,0x003fffff,0x007fffff,0x00ffffff,
 0x01ffffff,0x03ffffff,0x07ffffff,0x0fffffff,0x1fffffff,
 0x3fffffff,0x7fffffff,0xffffffff };

static const unsigned int mask8B[]=
{0x00,0x80,0xc0,0xe0,0xf0,0xf8,0xfc,0xfe,0xff};

void oggpack_writeinit(oggpack_buffer *b){
  memset(b,0,sizeof(*b));
  b->ptr=b->buffer=(unsigned char*)_ogg_malloc(BUFFER_INCREMENT);
  b->buffer[0]='\0';
  b->storage=BUFFER_INCREMENT;
}

void oggpackB_writeinit(oggpack_buffer *b){
  oggpack_writeinit(b);
}

int oggpack_writecheck(oggpack_buffer *b){
  if(!b->ptr || !b->storage)return -1;
  return 0;
}

int oggpackB_writecheck(oggpack_buffer *b){
  return oggpack_writecheck(b);
}

void oggpack_writetrunc(oggpack_buffer *b,long bits){
  long bytes=bits>>3;
  if(b->ptr){
    bits-=bytes*8;
    b->ptr=b->buffer+bytes;
    b->endbit=bits;
    b->endbyte=bytes;
    *b->ptr&=mask[bits];
  }
}

void oggpackB_writetrunc(oggpack_buffer *b,long bits){
  long bytes=bits>>3;
  if(b->ptr){
    bits-=bytes*8;
    b->ptr=b->buffer+bytes;
    b->endbit=bits;
    b->endbyte=bytes;
    *b->ptr&=mask8B[bits];
  }
}

/* Takes only up to 32 bits. */
void oggpack_write(oggpack_buffer *b,unsigned long value,int bits){
  if(bits<0 || bits>32) goto err;
  if(b->endbyte>=b->storage-4){
    void *ret;
    if(!b->ptr)return;
    if(b->storage>LONG_MAX-BUFFER_INCREMENT) goto err;
    ret=_ogg_realloc(b->buffer,b->storage+BUFFER_INCREMENT);
    if(!ret) goto err;
    b->buffer=(unsigned char*)ret;
    b->storage+=BUFFER_INCREMENT;
    b->ptr=b->buffer+b->endbyte;
  }

  value&=mask[bits];
  bits+=b->endbit;

  b->ptr[0]|=value<<b->endbit;

  if(bits>=8){
    b->ptr[1]=(unsigned char)(value>>(8-b->endbit));
    if(bits>=16){
      b->ptr[2]=(unsigned char)(value>>(16-b->endbit));
      if(bits>=24){
        b->ptr[3]=(unsigned char)(value>>(24-b->endbit));
        if(bits>=32){
          if(b->endbit)
            b->ptr[4]=(unsigned char)(value>>(32-b->endbit));
          else
            b->ptr[4]=0;
        }
      }
    }
  }

  b->endbyte+=bits/8;
  b->ptr+=bits/8;
  b->endbit=bits&7;
  return;
 err:
  oggpack_writeclear(b);
}

/* Takes only up to 32 bits. */
void oggpackB_write(oggpack_buffer *b,unsigned long value,int bits){
  if(bits<0 || bits>32) goto err;
  if(b->endbyte>=b->storage-4){
    void *ret;
    if(!b->ptr)return;
    if(b->storage>LONG_MAX-BUFFER_INCREMENT) goto err;
    ret=_ogg_realloc(b->buffer,b->storage+BUFFER_INCREMENT);
    if(!ret) goto err;
    b->buffer=(unsigned char*)ret;
    b->storage+=BUFFER_INCREMENT;
    b->ptr=b->buffer+b->endbyte;
  }

  value=(value&mask[bits])<<(32-bits);
  bits+=b->endbit;

  b->ptr[0]|=value>>(24+b->endbit);

  if(bits>=8){
    b->ptr[1]=(unsigned char)(value>>(16+b->endbit));
    if(bits>=16){
      b->ptr[2]=(unsigned char)(value>>(8+b->endbit));
      if(bits>=24){
        b->ptr[3]=(unsigned char)(value>>(b->endbit));
        if(bits>=32){
          if(b->endbit)
            b->ptr[4]=(unsigned char)(value<<(8-b->endbit));
          else
            b->ptr[4]=0;
        }
      }
    }
  }

  b->endbyte+=bits/8;
  b->ptr+=bits/8;
  b->endbit=bits&7;
  return;
 err:
  oggpack_writeclear(b);
}

void oggpack_writealign(oggpack_buffer *b){
  int bits=8-b->endbit;
  if(bits<8)
    oggpack_write(b,0,bits);
}

void oggpackB_writealign(oggpack_buffer *b){
  int bits=8-b->endbit;
  if(bits<8)
    oggpackB_write(b,0,bits);
}

static void oggpack_writecopy_helper(oggpack_buffer *b,
                                     void *source,
                                     long bits,
                                     void (*w)(oggpack_buffer *,
                                               unsigned long,
                                               int),
                                     int msb){
  unsigned char *ptr=(unsigned char *)source;

  long bytes=bits/8;
  long pbytes=(b->endbit+bits)/8;
  bits-=bytes*8;

  /* expand storage up-front */
  if(b->endbyte+pbytes>=b->storage){
    void *ret;
    if(!b->ptr) goto err;
    if(b->storage>b->endbyte+pbytes+BUFFER_INCREMENT) goto err;
    b->storage=b->endbyte+pbytes+BUFFER_INCREMENT;
    ret=_ogg_realloc(b->buffer,b->storage);
    if(!ret) goto err;
    b->buffer=(unsigned char*)ret;
    b->ptr=b->buffer+b->endbyte;
  }

  /* copy whole octets */
  if(b->endbit){
    int i;
    /* unaligned copy.  Do it the hard way. */
    for(i=0;i<bytes;i++)
      w(b,(unsigned long)(ptr[i]),8);
  }else{
    /* aligned block copy */
    memmove(b->ptr,source,bytes);
    b->ptr+=bytes;
    b->endbyte+=bytes;
    *b->ptr=0;
  }

  /* copy trailing bits */
  if(bits){
    if(msb)
      w(b,(unsigned long)(ptr[bytes]>>(8-bits)),bits);
    else
      w(b,(unsigned long)(ptr[bytes]),bits);
  }
  return;
 err:
  oggpack_writeclear(b);
}

void oggpack_writecopy(oggpack_buffer *b,void *source,long bits){
  oggpack_writecopy_helper(b,source,bits,oggpack_write,0);
}

void oggpackB_writecopy(oggpack_buffer *b,void *source,long bits){
  oggpack_writecopy_helper(b,source,bits,oggpackB_write,1);
}

void oggpack_reset(oggpack_buffer *b){
  if(!b->ptr)return;
  b->ptr=b->buffer;
  b->buffer[0]=0;
  b->endbit=b->endbyte=0;
}

void oggpackB_reset(oggpack_buffer *b){
  oggpack_reset(b);
}

void oggpack_writeclear(oggpack_buffer *b){
  if(b->buffer)_ogg_free(b->buffer);
  memset(b,0,sizeof(*b));
}

void oggpackB_writeclear(oggpack_buffer *b){
  oggpack_writeclear(b);
}

void oggpack_readinit(oggpack_buffer *b,unsigned char *buf,int bytes){
  memset(b,0,sizeof(*b));
  b->buffer=b->ptr=buf;
  b->storage=bytes;
}

void oggpackB_readinit(oggpack_buffer *b,unsigned char *buf,int bytes){
  oggpack_readinit(b,buf,bytes);
}

/* Read in bits without advancing the bitptr; bits <= 32 */
long oggpack_look(oggpack_buffer *b,int bits){
  unsigned long ret;
  unsigned long m;

  if(bits<0 || bits>32) return -1;
  m=mask[bits];
  bits+=b->endbit;

  if(b->endbyte >= b->storage-4){
    /* not the main path */
    if(b->endbyte > b->storage-((bits+7)>>3)) return -1;
    /* special case to avoid reading b->ptr[0], which might be past the end of
        the buffer; also skips some useless accounting */
    else if(!bits)return(0L);
  }

  ret=b->ptr[0]>>b->endbit;
  if(bits>8){
    ret|=b->ptr[1]<<(8-b->endbit);
    if(bits>16){
      ret|=b->ptr[2]<<(16-b->endbit);
      if(bits>24){
        ret|=b->ptr[3]<<(24-b->endbit);
        if(bits>32 && b->endbit)
          ret|=b->ptr[4]<<(32-b->endbit);
      }
    }
  }
  return(m&ret);
}

/* Read in bits without advancing the bitptr; bits <= 32 */
long oggpackB_look(oggpack_buffer *b,int bits){
  unsigned long ret;
  int m=32-bits;

  if(m<0 || m>32) return -1;
  bits+=b->endbit;

  if(b->endbyte >= b->storage-4){
    /* not the main path */
    if(b->endbyte > b->storage-((bits+7)>>3)) return -1;
    /* special case to avoid reading b->ptr[0], which might be past the end of
        the buffer; also skips some useless accounting */
    else if(!bits)return(0L);
  }

  ret=b->ptr[0]<<(24+b->endbit);
  if(bits>8){
    ret|=b->ptr[1]<<(16+b->endbit);
    if(bits>16){
      ret|=b->ptr[2]<<(8+b->endbit);
      if(bits>24){
        ret|=b->ptr[3]<<(b->endbit);
        if(bits>32 && b->endbit)
          ret|=b->ptr[4]>>(8-b->endbit);
      }
    }
  }
  return ((ret&0xffffffff)>>(m>>1))>>((m+1)>>1);
}

long oggpack_look1(oggpack_buffer *b){
  if(b->endbyte>=b->storage)return(-1);
  return((b->ptr[0]>>b->endbit)&1);
}

long oggpackB_look1(oggpack_buffer *b){
  if(b->endbyte>=b->storage)return(-1);
  return((b->ptr[0]>>(7-b->endbit))&1);
}

void oggpack_adv(oggpack_buffer *b,int bits){
  bits+=b->endbit;

  if(b->endbyte > b->storage-((bits+7)>>3)) goto overflow;

  b->ptr+=bits/8;
  b->endbyte+=bits/8;
  b->endbit=bits&7;
  return;

 overflow:
  b->ptr=NULL;
  b->endbyte=b->storage;
  b->endbit=1;
}

void oggpackB_adv(oggpack_buffer *b,int bits){
  oggpack_adv(b,bits);
}

void oggpack_adv1(oggpack_buffer *b){
  if(++(b->endbit)>7){
    b->endbit=0;
    b->ptr++;
    b->endbyte++;
  }
}

void oggpackB_adv1(oggpack_buffer *b){
  oggpack_adv1(b);
}

/* bits <= 32 */
long oggpack_read(oggpack_buffer *b,int bits){
  long ret;
  unsigned long m;

  if(bits<0 || bits>32) goto err;
  m=mask[bits];
  bits+=b->endbit;

  if(b->endbyte >= b->storage-4){
    /* not the main path */
    if(b->endbyte > b->storage-((bits+7)>>3)) goto overflow;
    /* special case to avoid reading b->ptr[0], which might be past the end of
        the buffer; also skips some useless accounting */
    else if(!bits)return(0L);
  }

  ret=b->ptr[0]>>b->endbit;
  if(bits>8){
    ret|=b->ptr[1]<<(8-b->endbit);
    if(bits>16){
      ret|=b->ptr[2]<<(16-b->endbit);
      if(bits>24){
        ret|=b->ptr[3]<<(24-b->endbit);
        if(bits>32 && b->endbit){
          ret|=b->ptr[4]<<(32-b->endbit);
        }
      }
    }
  }
  ret&=m;
  b->ptr+=bits/8;
  b->endbyte+=bits/8;
  b->endbit=bits&7;
  return ret;

 overflow:
 err:
  b->ptr=NULL;
  b->endbyte=b->storage;
  b->endbit=1;
  return -1L;
}

/* bits <= 32 */
long oggpackB_read(oggpack_buffer *b,int bits){
  long ret;
  long m=32-bits;

  if(m<0 || m>32) goto err;
  bits+=b->endbit;

  if(b->endbyte+4>=b->storage){
    /* not the main path */
    if(b->endbyte > b->storage-((bits+7)>>3)) goto overflow;
    /* special case to avoid reading b->ptr[0], which might be past the end of
        the buffer; also skips some useless accounting */
    else if(!bits)return(0L);
  }

  ret=b->ptr[0]<<(24+b->endbit);
  if(bits>8){
    ret|=b->ptr[1]<<(16+b->endbit);
    if(bits>16){
      ret|=b->ptr[2]<<(8+b->endbit);
      if(bits>24){
        ret|=b->ptr[3]<<(b->endbit);
        if(bits>32 && b->endbit)
          ret|=b->ptr[4]>>(8-b->endbit);
      }
    }
  }
  ret=((ret&0xffffffffUL)>>(m>>1))>>((m+1)>>1);

  b->ptr+=bits/8;
  b->endbyte+=bits/8;
  b->endbit=bits&7;
  return ret;

 overflow:
 err:
  b->ptr=NULL;
  b->endbyte=b->storage;
  b->endbit=1;
  return -1L;
}

long oggpack_read1(oggpack_buffer *b){
  long ret;

  if(b->endbyte >= b->storage) goto overflow;
  ret=(b->ptr[0]>>b->endbit)&1;

  b->endbit++;
  if(b->endbit>7){
    b->endbit=0;
    b->ptr++;
    b->endbyte++;
  }
  return ret;

 overflow:
  b->ptr=NULL;
  b->endbyte=b->storage;
  b->endbit=1;
  return -1L;
}

long oggpackB_read1(oggpack_buffer *b){
  long ret;

  if(b->endbyte >= b->storage) goto overflow;
  ret=(b->ptr[0]>>(7-b->endbit))&1;

  b->endbit++;
  if(b->endbit>7){
    b->endbit=0;
    b->ptr++;
    b->endbyte++;
  }
  return ret;

 overflow:
  b->ptr=NULL;
  b->endbyte=b->storage;
  b->endbit=1;
  return -1L;
}

long oggpack_bytes(oggpack_buffer *b){
  return(b->endbyte+(b->endbit+7)/8);
}

long oggpack_bits(oggpack_buffer *b){
  return(b->endbyte*8+b->endbit);
}

long oggpackB_bytes(oggpack_buffer *b){
  return oggpack_bytes(b);
}

long oggpackB_bits(oggpack_buffer *b){
  return oggpack_bits(b);
}

unsigned char *oggpack_get_buffer(oggpack_buffer *b){
  return(b->buffer);
}

unsigned char *oggpackB_get_buffer(oggpack_buffer *b){
  return oggpack_get_buffer(b);
}

/* Self test of the bitwise routines; everything else is based on
   them, so they damned well better be solid. */

#ifdef _V_SELFTEST
#include <stdio.h>

static int ilog(unsigned int v){
  int ret=0;
  while(v){
    ret++;
    v>>=1;
  }
  return(ret);
}

oggpack_buffer o;
oggpack_buffer r;

void report(char *in){
  fprintf(stderr,"%s",in);
  exit(1);
}

void cliptest(unsigned long *b,int vals,int bits,int *comp,int compsize){
  long bytes,i;
  unsigned char *buffer;

  oggpack_reset(&o);
  for(i=0;i<vals;i++)
    oggpack_write(&o,b[i],bits?bits:ilog(b[i]));
  buffer=oggpack_get_buffer(&o);
  bytes=oggpack_bytes(&o);
  if(bytes!=compsize)report("wrong number of bytes!\n");
  for(i=0;i<bytes;i++)if(buffer[i]!=comp[i]){
    for(i=0;i<bytes;i++)fprintf(stderr,"%x %x\n",(int)buffer[i],(int)comp[i]);
    report("wrote incorrect value!\n");
  }
  oggpack_readinit(&r,buffer,bytes);
  for(i=0;i<vals;i++){
    int tbit=bits?bits:ilog(b[i]);
    if(oggpack_look(&r,tbit)==-1)
      report("out of data!\n");
    if(oggpack_look(&r,tbit)!=(b[i]&mask[tbit]))
      report("looked at incorrect value!\n");
    if(tbit==1)
      if(oggpack_look1(&r)!=(b[i]&mask[tbit]))
        report("looked at single bit incorrect value!\n");
    if(tbit==1){
      if(oggpack_read1(&r)!=(b[i]&mask[tbit]))
        report("read incorrect single bit value!\n");
    }else{
    if(oggpack_read(&r,tbit)!=(b[i]&mask[tbit]))
      report("read incorrect value!\n");
    }
  }
  if(oggpack_bytes(&r)!=bytes)report("leftover bytes after read!\n");
}

void cliptestB(unsigned long *b,int vals,int bits,int *comp,int compsize){
  long bytes,i;
  unsigned char *buffer;

  oggpackB_reset(&o);
  for(i=0;i<vals;i++)
    oggpackB_write(&o,b[i],bits?bits:ilog(b[i]));
  buffer=oggpackB_get_buffer(&o);
  bytes=oggpackB_bytes(&o);
  if(bytes!=compsize)report("wrong number of bytes!\n");
  for(i=0;i<bytes;i++)if(buffer[i]!=comp[i]){
    for(i=0;i<bytes;i++)fprintf(stderr,"%x %x\n",(int)buffer[i],(int)comp[i]);
    report("wrote incorrect value!\n");
  }
  oggpackB_readinit(&r,buffer,bytes);
  for(i=0;i<vals;i++){
    int tbit=bits?bits:ilog(b[i]);
    if(oggpackB_look(&r,tbit)==-1)
      report("out of data!\n");
    if(oggpackB_look(&r,tbit)!=(b[i]&mask[tbit]))
      report("looked at incorrect value!\n");
    if(tbit==1)
      if(oggpackB_look1(&r)!=(b[i]&mask[tbit]))
        report("looked at single bit incorrect value!\n");
    if(tbit==1){
      if(oggpackB_read1(&r)!=(b[i]&mask[tbit]))
        report("read incorrect single bit value!\n");
    }else{
    if(oggpackB_read(&r,tbit)!=(b[i]&mask[tbit]))
      report("read incorrect value!\n");
    }
  }
  if(oggpackB_bytes(&r)!=bytes)report("leftover bytes after read!\n");
}

void copytest(int prefill, int copy){
  oggpack_buffer source_write;
  oggpack_buffer dest_write;
  oggpack_buffer source_read;
  oggpack_buffer dest_read;
  unsigned char *source;
  unsigned char *dest;
  long source_bytes,dest_bytes;
  int i;

  oggpack_writeinit(&source_write);
  oggpack_writeinit(&dest_write);

  for(i=0;i<(prefill+copy+7)/8;i++)
    oggpack_write(&source_write,(i^0x5a)&0xff,8);
  source=oggpack_get_buffer(&source_write);
  source_bytes=oggpack_bytes(&source_write);

  /* prefill */
  oggpack_writecopy(&dest_write,source,prefill);

  /* check buffers; verify end byte masking */
  dest=oggpack_get_buffer(&dest_write);
  dest_bytes=oggpack_bytes(&dest_write);
  if(dest_bytes!=(prefill+7)/8){
    fprintf(stderr,"wrong number of bytes after prefill! %ld!=%d\n",dest_bytes,(prefill+7)/8);
    exit(1);
  }
  oggpack_readinit(&source_read,source,source_bytes);
  oggpack_readinit(&dest_read,dest,dest_bytes);

  for(i=0;i<prefill;i+=8){
    int s=oggpack_read(&source_read,prefill-i<8?prefill-i:8);
    int d=oggpack_read(&dest_read,prefill-i<8?prefill-i:8);
    if(s!=d){
      fprintf(stderr,"prefill=%d mismatch! byte %d, %x!=%x\n",prefill,i/8,s,d);
      exit(1);
    }
  }
  if(prefill<dest_bytes){
    if(oggpack_read(&dest_read,dest_bytes-prefill)!=0){
      fprintf(stderr,"prefill=%d mismatch! trailing bits not zero\n",prefill);
      exit(1);
    }
  }

  /* second copy */
  oggpack_writecopy(&dest_write,source,copy);

  /* check buffers; verify end byte masking */
  dest=oggpack_get_buffer(&dest_write);
  dest_bytes=oggpack_bytes(&dest_write);
  if(dest_bytes!=(copy+prefill+7)/8){
    fprintf(stderr,"wrong number of bytes after prefill+copy! %ld!=%d\n",dest_bytes,(copy+prefill+7)/8);
    exit(1);
  }
  oggpack_readinit(&source_read,source,source_bytes);
  oggpack_readinit(&dest_read,dest,dest_bytes);

  for(i=0;i<prefill;i+=8){
    int s=oggpack_read(&source_read,prefill-i<8?prefill-i:8);
    int d=oggpack_read(&dest_read,prefill-i<8?prefill-i:8);
    if(s!=d){
      fprintf(stderr,"prefill=%d mismatch! byte %d, %x!=%x\n",prefill,i/8,s,d);
      exit(1);
    }
  }

  oggpack_readinit(&source_read,source,source_bytes);
  for(i=0;i<copy;i+=8){
    int s=oggpack_read(&source_read,copy-i<8?copy-i:8);
    int d=oggpack_read(&dest_read,copy-i<8?copy-i:8);
    if(s!=d){
      fprintf(stderr,"prefill=%d copy=%d mismatch! byte %d, %x!=%x\n",prefill,copy,i/8,s,d);
      exit(1);
    }
  }

  if(copy+prefill<dest_bytes){
    if(oggpack_read(&dest_read,dest_bytes-copy-prefill)!=0){
      fprintf(stderr,"prefill=%d copy=%d mismatch! trailing bits not zero\n",prefill,copy);
      exit(1);
    }
  }

  oggpack_writeclear(&source_write);
  oggpack_writeclear(&dest_write);


}

void copytestB(int prefill, int copy){
  oggpack_buffer source_write;
  oggpack_buffer dest_write;
  oggpack_buffer source_read;
  oggpack_buffer dest_read;
  unsigned char *source;
  unsigned char *dest;
  long source_bytes,dest_bytes;
  int i;

  oggpackB_writeinit(&source_write);
  oggpackB_writeinit(&dest_write);

  for(i=0;i<(prefill+copy+7)/8;i++)
    oggpackB_write(&source_write,(i^0x5a)&0xff,8);
  source=oggpackB_get_buffer(&source_write);
  source_bytes=oggpackB_bytes(&source_write);

  /* prefill */
  oggpackB_writecopy(&dest_write,source,prefill);

  /* check buffers; verify end byte masking */
  dest=oggpackB_get_buffer(&dest_write);
  dest_bytes=oggpackB_bytes(&dest_write);
  if(dest_bytes!=(prefill+7)/8){
    fprintf(stderr,"wrong number of bytes after prefill! %ld!=%d\n",dest_bytes,(prefill+7)/8);
    exit(1);
  }
  oggpackB_readinit(&source_read,source,source_bytes);
  oggpackB_readinit(&dest_read,dest,dest_bytes);

  for(i=0;i<prefill;i+=8){
    int s=oggpackB_read(&source_read,prefill-i<8?prefill-i:8);
    int d=oggpackB_read(&dest_read,prefill-i<8?prefill-i:8);
    if(s!=d){
      fprintf(stderr,"prefill=%d mismatch! byte %d, %x!=%x\n",prefill,i/8,s,d);
      exit(1);
    }
  }
  if(prefill<dest_bytes){
    if(oggpackB_read(&dest_read,dest_bytes-prefill)!=0){
      fprintf(stderr,"prefill=%d mismatch! trailing bits not zero\n",prefill);
      exit(1);
    }
  }

  /* second copy */
  oggpackB_writecopy(&dest_write,source,copy);

  /* check buffers; verify end byte masking */
  dest=oggpackB_get_buffer(&dest_write);
  dest_bytes=oggpackB_bytes(&dest_write);
  if(dest_bytes!=(copy+prefill+7)/8){
    fprintf(stderr,"wrong number of bytes after prefill+copy! %ld!=%d\n",dest_bytes,(copy+prefill+7)/8);
    exit(1);
  }
  oggpackB_readinit(&source_read,source,source_bytes);
  oggpackB_readinit(&dest_read,dest,dest_bytes);

  for(i=0;i<prefill;i+=8){
    int s=oggpackB_read(&source_read,prefill-i<8?prefill-i:8);
    int d=oggpackB_read(&dest_read,prefill-i<8?prefill-i:8);
    if(s!=d){
      fprintf(stderr,"prefill=%d mismatch! byte %d, %x!=%x\n",prefill,i/8,s,d);
      exit(1);
    }
  }

  oggpackB_readinit(&source_read,source,source_bytes);
  for(i=0;i<copy;i+=8){
    int s=oggpackB_read(&source_read,copy-i<8?copy-i:8);
    int d=oggpackB_read(&dest_read,copy-i<8?copy-i:8);
    if(s!=d){
      fprintf(stderr,"prefill=%d copy=%d mismatch! byte %d, %x!=%x\n",prefill,copy,i/8,s,d);
      exit(1);
    }
  }

  if(copy+prefill<dest_bytes){
    if(oggpackB_read(&dest_read,dest_bytes-copy-prefill)!=0){
      fprintf(stderr,"prefill=%d copy=%d mismatch! trailing bits not zero\n",prefill,copy);
      exit(1);
    }
  }

  oggpackB_writeclear(&source_write);
  oggpackB_writeclear(&dest_write);

}

int main(void){
  unsigned char *buffer;
  long bytes,i,j;
  static unsigned long testbuffer1[]=
    {18,12,103948,4325,543,76,432,52,3,65,4,56,32,42,34,21,1,23,32,546,456,7,
       567,56,8,8,55,3,52,342,341,4,265,7,67,86,2199,21,7,1,5,1,4};
  int test1size=43;

  static unsigned long testbuffer2[]=
    {216531625L,1237861823,56732452,131,3212421,12325343,34547562,12313212,
       1233432,534,5,346435231,14436467,7869299,76326614,167548585,
       85525151,0,12321,1,349528352};
  int test2size=21;

  static unsigned long testbuffer3[]=
    {1,0,14,0,1,0,12,0,1,0,0,0,1,1,0,1,0,1,0,1,0,1,0,1,0,1,0,0,1,1,1,1,1,0,0,1,
       0,1,30,1,1,1,0,0,1,0,0,0,12,0,11,0,1,0,0,1};
  int test3size=56;

  static unsigned long large[]=
    {2136531625L,2137861823,56732452,131,3212421,12325343,34547562,12313212,
       1233432,534,5,2146435231,14436467,7869299,76326614,167548585,
       85525151,0,12321,1,2146528352};

  int onesize=33;
  static int one[33]={146,25,44,151,195,15,153,176,233,131,196,65,85,172,47,40,
                    34,242,223,136,35,222,211,86,171,50,225,135,214,75,172,
                    223,4};
  static int oneB[33]={150,101,131,33,203,15,204,216,105,193,156,65,84,85,222,
                       8,139,145,227,126,34,55,244,171,85,100,39,195,173,18,
                       245,251,128};

  int twosize=6;
  static int two[6]={61,255,255,251,231,29};
  static int twoB[6]={247,63,255,253,249,120};

  int threesize=54;
  static int three[54]={169,2,232,252,91,132,156,36,89,13,123,176,144,32,254,
                      142,224,85,59,121,144,79,124,23,67,90,90,216,79,23,83,
                      58,135,196,61,55,129,183,54,101,100,170,37,127,126,10,
                      100,52,4,14,18,86,77,1};
  static int threeB[54]={206,128,42,153,57,8,183,251,13,89,36,30,32,144,183,
                         130,59,240,121,59,85,223,19,228,180,134,33,107,74,98,
                         233,253,196,135,63,2,110,114,50,155,90,127,37,170,104,
                         200,20,254,4,58,106,176,144,0};

  int foursize=38;
  static int four[38]={18,6,163,252,97,194,104,131,32,1,7,82,137,42,129,11,72,
                     132,60,220,112,8,196,109,64,179,86,9,137,195,208,122,169,
                     28,2,133,0,1};
  static int fourB[38]={36,48,102,83,243,24,52,7,4,35,132,10,145,21,2,93,2,41,
                        1,219,184,16,33,184,54,149,170,132,18,30,29,98,229,67,
                        129,10,4,32};

  int fivesize=45;
  static int five[45]={169,2,126,139,144,172,30,4,80,72,240,59,130,218,73,62,
                     241,24,210,44,4,20,0,248,116,49,135,100,110,130,181,169,
                     84,75,159,2,1,0,132,192,8,0,0,18,22};
  static int fiveB[45]={1,84,145,111,245,100,128,8,56,36,40,71,126,78,213,226,
                        124,105,12,0,133,128,0,162,233,242,67,152,77,205,77,
                        172,150,169,129,79,128,0,6,4,32,0,27,9,0};

  int sixsize=7;
  static int six[7]={17,177,170,242,169,19,148};
  static int sixB[7]={136,141,85,79,149,200,41};

  /* Test read/write together */
  /* Later we test against pregenerated bitstreams */
  oggpack_writeinit(&o);

  fprintf(stderr,"\nSmall preclipped packing (LSb): ");
  cliptest(testbuffer1,test1size,0,one,onesize);
  fprintf(stderr,"ok.");

  fprintf(stderr,"\nNull bit call (LSb): ");
  cliptest(testbuffer3,test3size,0,two,twosize);
  fprintf(stderr,"ok.");

  fprintf(stderr,"\nLarge preclipped packing (LSb): ");
  cliptest(testbuffer2,test2size,0,three,threesize);
  fprintf(stderr,"ok.");

  fprintf(stderr,"\n32 bit preclipped packing (LSb): ");
  oggpack_reset(&o);
  for(i=0;i<test2size;i++)
    oggpack_write(&o,large[i],32);
  buffer=oggpack_get_buffer(&o);
  bytes=oggpack_bytes(&o);
  oggpack_readinit(&r,buffer,bytes);
  for(i=0;i<test2size;i++){
    if(oggpack_look(&r,32)==-1)report("out of data. failed!");
    if(oggpack_look(&r,32)!=large[i]){
      fprintf(stderr,"%ld != %lu (%lx!=%lx):",oggpack_look(&r,32),large[i],
              oggpack_look(&r,32),large[i]);
      report("read incorrect value!\n");
    }
    oggpack_adv(&r,32);
  }
  if(oggpack_bytes(&r)!=bytes)report("leftover bytes after read!\n");
  fprintf(stderr,"ok.");

  fprintf(stderr,"\nSmall unclipped packing (LSb): ");
  cliptest(testbuffer1,test1size,7,four,foursize);
  fprintf(stderr,"ok.");

  fprintf(stderr,"\nLarge unclipped packing (LSb): ");
  cliptest(testbuffer2,test2size,17,five,fivesize);
  fprintf(stderr,"ok.");

  fprintf(stderr,"\nSingle bit unclipped packing (LSb): ");
  cliptest(testbuffer3,test3size,1,six,sixsize);
  fprintf(stderr,"ok.");

  fprintf(stderr,"\nTesting read past end (LSb): ");
  oggpack_readinit(&r,(unsigned char *)"\0\0\0\0\0\0\0\0",8);
  for(i=0;i<64;i++){
    if(oggpack_read(&r,1)!=0){
      fprintf(stderr,"failed; got -1 prematurely.\n");
      exit(1);
    }
  }
  if(oggpack_look(&r,1)!=-1 ||
     oggpack_read(&r,1)!=-1){
      fprintf(stderr,"failed; read past end without -1.\n");
      exit(1);
  }
  oggpack_readinit(&r,(unsigned char *)"\0\0\0\0\0\0\0\0",8);
  if(oggpack_read(&r,30)!=0 || oggpack_read(&r,16)!=0){
      fprintf(stderr,"failed 2; got -1 prematurely.\n");
      exit(1);
  }

  if(oggpack_look(&r,18)!=0 ||
     oggpack_look(&r,18)!=0){
    fprintf(stderr,"failed 3; got -1 prematurely.\n");
      exit(1);
  }
  if(oggpack_look(&r,19)!=-1 ||
     oggpack_look(&r,19)!=-1){
    fprintf(stderr,"failed; read past end without -1.\n");
      exit(1);
  }
  if(oggpack_look(&r,32)!=-1 ||
     oggpack_look(&r,32)!=-1){
    fprintf(stderr,"failed; read past end without -1.\n");
      exit(1);
  }
  oggpack_writeclear(&o);
  fprintf(stderr,"ok.");

  /* this is partly glassbox; we're mostly concerned about the allocation boundaries */

  fprintf(stderr,"\nTesting aligned writecopies (LSb): ");
  for(i=0;i<71;i++)
    for(j=0;j<5;j++)
      copytest(j*8,i);
  for(i=BUFFER_INCREMENT*8-71;i<BUFFER_INCREMENT*8+71;i++)
    for(j=0;j<5;j++)
      copytest(j*8,i);
  fprintf(stderr,"ok.      ");

  fprintf(stderr,"\nTesting unaligned writecopies (LSb): ");
  for(i=0;i<71;i++)
    for(j=1;j<40;j++)
      if(j&0x7)
        copytest(j,i);
  for(i=BUFFER_INCREMENT*8-71;i<BUFFER_INCREMENT*8+71;i++)
    for(j=1;j<40;j++)
      if(j&0x7)
        copytest(j,i);
  
  fprintf(stderr,"ok.      \n");


  /********** lazy, cut-n-paste retest with MSb packing ***********/

  /* Test read/write together */
  /* Later we test against pregenerated bitstreams */
  oggpackB_writeinit(&o);

  fprintf(stderr,"\nSmall preclipped packing (MSb): ");
  cliptestB(testbuffer1,test1size,0,oneB,onesize);
  fprintf(stderr,"ok.");

  fprintf(stderr,"\nNull bit call (MSb): ");
  cliptestB(testbuffer3,test3size,0,twoB,twosize);
  fprintf(stderr,"ok.");

  fprintf(stderr,"\nLarge preclipped packing (MSb): ");
  cliptestB(testbuffer2,test2size,0,threeB,threesize);
  fprintf(stderr,"ok.");

  fprintf(stderr,"\n32 bit preclipped packing (MSb): ");
  oggpackB_reset(&o);
  for(i=0;i<test2size;i++)
    oggpackB_write(&o,large[i],32);
  buffer=oggpackB_get_buffer(&o);
  bytes=oggpackB_bytes(&o);
  oggpackB_readinit(&r,buffer,bytes);
  for(i=0;i<test2size;i++){
    if(oggpackB_look(&r,32)==-1)report("out of data. failed!");
    if(oggpackB_look(&r,32)!=large[i]){
      fprintf(stderr,"%ld != %lu (%lx!=%lx):",oggpackB_look(&r,32),large[i],
              oggpackB_look(&r,32),large[i]);
      report("read incorrect value!\n");
    }
    oggpackB_adv(&r,32);
  }
  if(oggpackB_bytes(&r)!=bytes)report("leftover bytes after read!\n");
  fprintf(stderr,"ok.");

  fprintf(stderr,"\nSmall unclipped packing (MSb): ");
  cliptestB(testbuffer1,test1size,7,fourB,foursize);
  fprintf(stderr,"ok.");

  fprintf(stderr,"\nLarge unclipped packing (MSb): ");
  cliptestB(testbuffer2,test2size,17,fiveB,fivesize);
  fprintf(stderr,"ok.");

  fprintf(stderr,"\nSingle bit unclipped packing (MSb): ");
  cliptestB(testbuffer3,test3size,1,sixB,sixsize);
  fprintf(stderr,"ok.");

  fprintf(stderr,"\nTesting read past end (MSb): ");
  oggpackB_readinit(&r,(unsigned char *)"\0\0\0\0\0\0\0\0",8);
  for(i=0;i<64;i++){
    if(oggpackB_read(&r,1)!=0){
      fprintf(stderr,"failed; got -1 prematurely.\n");
      exit(1);
    }
  }
  if(oggpackB_look(&r,1)!=-1 ||
     oggpackB_read(&r,1)!=-1){
      fprintf(stderr,"failed; read past end without -1.\n");
      exit(1);
  }
  oggpackB_readinit(&r,(unsigned char *)"\0\0\0\0\0\0\0\0",8);
  if(oggpackB_read(&r,30)!=0 || oggpackB_read(&r,16)!=0){
      fprintf(stderr,"failed 2; got -1 prematurely.\n");
      exit(1);
  }

  if(oggpackB_look(&r,18)!=0 ||
     oggpackB_look(&r,18)!=0){
    fprintf(stderr,"failed 3; got -1 prematurely.\n");
      exit(1);
  }
  if(oggpackB_look(&r,19)!=-1 ||
     oggpackB_look(&r,19)!=-1){
    fprintf(stderr,"failed; read past end without -1.\n");
      exit(1);
  }
  if(oggpackB_look(&r,32)!=-1 ||
     oggpackB_look(&r,32)!=-1){
    fprintf(stderr,"failed; read past end without -1.\n");
      exit(1);
  }
  fprintf(stderr,"ok.");
  oggpackB_writeclear(&o);

  /* this is partly glassbox; we're mostly concerned about the allocation boundaries */

  fprintf(stderr,"\nTesting aligned writecopies (MSb): ");
  for(i=0;i<71;i++)
    for(j=0;j<5;j++)
      copytestB(j*8,i);
  for(i=BUFFER_INCREMENT*8-71;i<BUFFER_INCREMENT*8+71;i++)
    for(j=0;j<5;j++)
      copytestB(j*8,i);
  fprintf(stderr,"ok.      ");

  fprintf(stderr,"\nTesting unaligned writecopies (MSb): ");
  for(i=0;i<71;i++)
    for(j=1;j<40;j++)
      if(j&0x7)
        copytestB(j,i);
  for(i=BUFFER_INCREMENT*8-71;i<BUFFER_INCREMENT*8+71;i++)
    for(j=1;j<40;j++)
      if(j&0x7)
        copytestB(j,i);
  
  fprintf(stderr,"ok.      \n\n");

  return(0);
}
#endif  /* _V_SELFTEST */

#undef BUFFER_INCREMENT
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE Ogg CONTAINER SOURCE CODE.              *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2018             *
 * by the Xiph.Org Foundation http://www.xiph.org/                  *
 *                                                                  *
 ********************************************************************

 function: code raw packets into framed OggSquish stream and
           decode Ogg streams back into raw packets

 note: The CRC code is directly derived from public domain code by
 Ross Williams (ross@guest.adelaide.edu.au).  See docs/framing.html
 for details.

 ********************************************************************/

#ifdef HAVE_CONFIG_H
/*#include "config.h"*/
#endif

#include <stdlib.h>
#include <limits.h>
#include <string.h>
/*#include <ogg/ogg.h>*/

/* A complete description of Ogg framing exists in docs/framing.html */

int ogg_page_version(const ogg_page *og){
  return((int)(og->header[4]));
}

int ogg_page_continued(const ogg_page *og){
  return((int)(og->header[5]&0x01));
}

int ogg_page_bos(const ogg_page *og){
  return((int)(og->header[5]&0x02));
}

int ogg_page_eos(const ogg_page *og){
  return((int)(og->header[5]&0x04));
}

ogg_int64_t ogg_page_granulepos(const ogg_page *og){
  unsigned char *page=og->header;
  ogg_uint64_t granulepos=page[13]&(0xff);
  granulepos= (granulepos<<8)|(page[12]&0xff);
  granulepos= (granulepos<<8)|(page[11]&0xff);
  granulepos= (granulepos<<8)|(page[10]&0xff);
  granulepos= (granulepos<<8)|(page[9]&0xff);
  granulepos= (granulepos<<8)|(page[8]&0xff);
  granulepos= (granulepos<<8)|(page[7]&0xff);
  granulepos= (granulepos<<8)|(page[6]&0xff);
  return((ogg_int64_t)granulepos);
}

int ogg_page_serialno(const ogg_page *og){
  return((int)((ogg_uint32_t)og->header[14]) |
              ((ogg_uint32_t)og->header[15]<<8) |
              ((ogg_uint32_t)og->header[16]<<16) |
              ((ogg_uint32_t)og->header[17]<<24));
}

long ogg_page_pageno(const ogg_page *og){
  return((long)((ogg_uint32_t)og->header[18]) |
               ((ogg_uint32_t)og->header[19]<<8) |
               ((ogg_uint32_t)og->header[20]<<16) |
               ((ogg_uint32_t)og->header[21]<<24));
}



/* returns the number of packets that are completed on this page (if
   the leading packet is begun on a previous page, but ends on this
   page, it's counted */

/* NOTE:
   If a page consists of a packet begun on a previous page, and a new
   packet begun (but not completed) on this page, the return will be:
     ogg_page_packets(page)   ==1,
     ogg_page_continued(page) !=0

   If a page happens to be a single packet that was begun on a
   previous page, and spans to the next page (in the case of a three or
   more page packet), the return will be:
     ogg_page_packets(page)   ==0,
     ogg_page_continued(page) !=0
*/

int ogg_page_packets(const ogg_page *og){
  int i,n=og->header[26],count=0;
  for(i=0;i<n;i++)
    if(og->header[27+i]<255)count++;
  return(count);
}


#if 0
/* helper to initialize lookup for direct-table CRC (illustrative; we
   use the static init in crctable.h) */

static void _ogg_crc_init(){
  int i, j;
  ogg_uint32_t polynomial, crc;
  polynomial = 0x04c11db7; /* The same as the ethernet generator
                              polynomial, although we use an
                              unreflected alg and an init/final
                              of 0, not 0xffffffff */
  for (i = 0; i <= 0xFF; i++){
    crc = i << 24;

    for (j = 0; j < 8; j++)
      crc = (crc << 1) ^ (crc & (1 << 31) ? polynomial : 0);

    crc_lookup[0][i] = crc;
  }

  for (i = 0; i <= 0xFF; i++)
    for (j = 1; j < 8; j++)
      crc_lookup[j][i] = crc_lookup[0][(crc_lookup[j - 1][i] >> 24) & 0xFF] ^ (crc_lookup[j - 1][i] << 8);
}
#endif

/*#include "crctable.h"*/
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE Ogg CONTAINER SOURCE CODE.              *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2018             *
 * by the Xiph.Org Foundation http://www.xiph.org/                  *
 *                                                                  *
 ********************************************************************/

/*#include <ogg/os_types.h>*/

static const ogg_uint32_t crc_lookup[8][256]={
{0x00000000,0x04c11db7,0x09823b6e,0x0d4326d9,0x130476dc,0x17c56b6b,0x1a864db2,0x1e475005,
 0x2608edb8,0x22c9f00f,0x2f8ad6d6,0x2b4bcb61,0x350c9b64,0x31cd86d3,0x3c8ea00a,0x384fbdbd,
 0x4c11db70,0x48d0c6c7,0x4593e01e,0x4152fda9,0x5f15adac,0x5bd4b01b,0x569796c2,0x52568b75,
 0x6a1936c8,0x6ed82b7f,0x639b0da6,0x675a1011,0x791d4014,0x7ddc5da3,0x709f7b7a,0x745e66cd,
 0x9823b6e0,0x9ce2ab57,0x91a18d8e,0x95609039,0x8b27c03c,0x8fe6dd8b,0x82a5fb52,0x8664e6e5,
 0xbe2b5b58,0xbaea46ef,0xb7a96036,0xb3687d81,0xad2f2d84,0xa9ee3033,0xa4ad16ea,0xa06c0b5d,
 0xd4326d90,0xd0f37027,0xddb056fe,0xd9714b49,0xc7361b4c,0xc3f706fb,0xceb42022,0xca753d95,
 0xf23a8028,0xf6fb9d9f,0xfbb8bb46,0xff79a6f1,0xe13ef6f4,0xe5ffeb43,0xe8bccd9a,0xec7dd02d,
 0x34867077,0x30476dc0,0x3d044b19,0x39c556ae,0x278206ab,0x23431b1c,0x2e003dc5,0x2ac12072,
 0x128e9dcf,0x164f8078,0x1b0ca6a1,0x1fcdbb16,0x018aeb13,0x054bf6a4,0x0808d07d,0x0cc9cdca,
 0x7897ab07,0x7c56b6b0,0x71159069,0x75d48dde,0x6b93dddb,0x6f52c06c,0x6211e6b5,0x66d0fb02,
 0x5e9f46bf,0x5a5e5b08,0x571d7dd1,0x53dc6066,0x4d9b3063,0x495a2dd4,0x44190b0d,0x40d816ba,
 0xaca5c697,0xa864db20,0xa527fdf9,0xa1e6e04e,0xbfa1b04b,0xbb60adfc,0xb6238b25,0xb2e29692,
 0x8aad2b2f,0x8e6c3698,0x832f1041,0x87ee0df6,0x99a95df3,0x9d684044,0x902b669d,0x94ea7b2a,
 0xe0b41de7,0xe4750050,0xe9362689,0xedf73b3e,0xf3b06b3b,0xf771768c,0xfa325055,0xfef34de2,
 0xc6bcf05f,0xc27dede8,0xcf3ecb31,0xcbffd686,0xd5b88683,0xd1799b34,0xdc3abded,0xd8fba05a,
 0x690ce0ee,0x6dcdfd59,0x608edb80,0x644fc637,0x7a089632,0x7ec98b85,0x738aad5c,0x774bb0eb,
 0x4f040d56,0x4bc510e1,0x46863638,0x42472b8f,0x5c007b8a,0x58c1663d,0x558240e4,0x51435d53,
 0x251d3b9e,0x21dc2629,0x2c9f00f0,0x285e1d47,0x36194d42,0x32d850f5,0x3f9b762c,0x3b5a6b9b,
 0x0315d626,0x07d4cb91,0x0a97ed48,0x0e56f0ff,0x1011a0fa,0x14d0bd4d,0x19939b94,0x1d528623,
 0xf12f560e,0xf5ee4bb9,0xf8ad6d60,0xfc6c70d7,0xe22b20d2,0xe6ea3d65,0xeba91bbc,0xef68060b,
 0xd727bbb6,0xd3e6a601,0xdea580d8,0xda649d6f,0xc423cd6a,0xc0e2d0dd,0xcda1f604,0xc960ebb3,
 0xbd3e8d7e,0xb9ff90c9,0xb4bcb610,0xb07daba7,0xae3afba2,0xaafbe615,0xa7b8c0cc,0xa379dd7b,
 0x9b3660c6,0x9ff77d71,0x92b45ba8,0x9675461f,0x8832161a,0x8cf30bad,0x81b02d74,0x857130c3,
 0x5d8a9099,0x594b8d2e,0x5408abf7,0x50c9b640,0x4e8ee645,0x4a4ffbf2,0x470cdd2b,0x43cdc09c,
 0x7b827d21,0x7f436096,0x7200464f,0x76c15bf8,0x68860bfd,0x6c47164a,0x61043093,0x65c52d24,
 0x119b4be9,0x155a565e,0x18197087,0x1cd86d30,0x029f3d35,0x065e2082,0x0b1d065b,0x0fdc1bec,
 0x3793a651,0x3352bbe6,0x3e119d3f,0x3ad08088,0x2497d08d,0x2056cd3a,0x2d15ebe3,0x29d4f654,
 0xc5a92679,0xc1683bce,0xcc2b1d17,0xc8ea00a0,0xd6ad50a5,0xd26c4d12,0xdf2f6bcb,0xdbee767c,
 0xe3a1cbc1,0xe760d676,0xea23f0af,0xeee2ed18,0xf0a5bd1d,0xf464a0aa,0xf9278673,0xfde69bc4,
 0x89b8fd09,0x8d79e0be,0x803ac667,0x84fbdbd0,0x9abc8bd5,0x9e7d9662,0x933eb0bb,0x97ffad0c,
 0xafb010b1,0xab710d06,0xa6322bdf,0xa2f33668,0xbcb4666d,0xb8757bda,0xb5365d03,0xb1f740b4},

{0x00000000,0xd219c1dc,0xa0f29e0f,0x72eb5fd3,0x452421a9,0x973de075,0xe5d6bfa6,0x37cf7e7a,
 0x8a484352,0x5851828e,0x2abadd5d,0xf8a31c81,0xcf6c62fb,0x1d75a327,0x6f9efcf4,0xbd873d28,
 0x10519b13,0xc2485acf,0xb0a3051c,0x62bac4c0,0x5575baba,0x876c7b66,0xf58724b5,0x279ee569,
 0x9a19d841,0x4800199d,0x3aeb464e,0xe8f28792,0xdf3df9e8,0x0d243834,0x7fcf67e7,0xadd6a63b,
 0x20a33626,0xf2baf7fa,0x8051a829,0x524869f5,0x6587178f,0xb79ed653,0xc5758980,0x176c485c,
 0xaaeb7574,0x78f2b4a8,0x0a19eb7b,0xd8002aa7,0xefcf54dd,0x3dd69501,0x4f3dcad2,0x9d240b0e,
 0x30f2ad35,0xe2eb6ce9,0x9000333a,0x4219f2e6,0x75d68c9c,0xa7cf4d40,0xd5241293,0x073dd34f,
 0xbabaee67,0x68a32fbb,0x1a487068,0xc851b1b4,0xff9ecfce,0x2d870e12,0x5f6c51c1,0x8d75901d,
 0x41466c4c,0x935fad90,0xe1b4f243,0x33ad339f,0x04624de5,0xd67b8c39,0xa490d3ea,0x76891236,
 0xcb0e2f1e,0x1917eec2,0x6bfcb111,0xb9e570cd,0x8e2a0eb7,0x5c33cf6b,0x2ed890b8,0xfcc15164,
 0x5117f75f,0x830e3683,0xf1e56950,0x23fca88c,0x1433d6f6,0xc62a172a,0xb4c148f9,0x66d88925,
 0xdb5fb40d,0x094675d1,0x7bad2a02,0xa9b4ebde,0x9e7b95a4,0x4c625478,0x3e890bab,0xec90ca77,
 0x61e55a6a,0xb3fc9bb6,0xc117c465,0x130e05b9,0x24c17bc3,0xf6d8ba1f,0x8433e5cc,0x562a2410,
 0xebad1938,0x39b4d8e4,0x4b5f8737,0x994646eb,0xae893891,0x7c90f94d,0x0e7ba69e,0xdc626742,
 0x71b4c179,0xa3ad00a5,0xd1465f76,0x035f9eaa,0x3490e0d0,0xe689210c,0x94627edf,0x467bbf03,
 0xfbfc822b,0x29e543f7,0x5b0e1c24,0x8917ddf8,0xbed8a382,0x6cc1625e,0x1e2a3d8d,0xcc33fc51,
 0x828cd898,0x50951944,0x227e4697,0xf067874b,0xc7a8f931,0x15b138ed,0x675a673e,0xb543a6e2,
 0x08c49bca,0xdadd5a16,0xa83605c5,0x7a2fc419,0x4de0ba63,0x9ff97bbf,0xed12246c,0x3f0be5b0,
 0x92dd438b,0x40c48257,0x322fdd84,0xe0361c58,0xd7f96222,0x05e0a3fe,0x770bfc2d,0xa5123df1,
 0x189500d9,0xca8cc105,0xb8679ed6,0x6a7e5f0a,0x5db12170,0x8fa8e0ac,0xfd43bf7f,0x2f5a7ea3,
 0xa22feebe,0x70362f62,0x02dd70b1,0xd0c4b16d,0xe70bcf17,0x35120ecb,0x47f95118,0x95e090c4,
 0x2867adec,0xfa7e6c30,0x889533e3,0x5a8cf23f,0x6d438c45,0xbf5a4d99,0xcdb1124a,0x1fa8d396,
 0xb27e75ad,0x6067b471,0x128ceba2,0xc0952a7e,0xf75a5404,0x254395d8,0x57a8ca0b,0x85b10bd7,
 0x383636ff,0xea2ff723,0x98c4a8f0,0x4add692c,0x7d121756,0xaf0bd68a,0xdde08959,0x0ff94885,
 0xc3cab4d4,0x11d37508,0x63382adb,0xb121eb07,0x86ee957d,0x54f754a1,0x261c0b72,0xf405caae,
 0x4982f786,0x9b9b365a,0xe9706989,0x3b69a855,0x0ca6d62f,0xdebf17f3,0xac544820,0x7e4d89fc,
 0xd39b2fc7,0x0182ee1b,0x7369b1c8,0xa1707014,0x96bf0e6e,0x44a6cfb2,0x364d9061,0xe45451bd,
 0x59d36c95,0x8bcaad49,0xf921f29a,0x2b383346,0x1cf74d3c,0xceee8ce0,0xbc05d333,0x6e1c12ef,
 0xe36982f2,0x3170432e,0x439b1cfd,0x9182dd21,0xa64da35b,0x74546287,0x06bf3d54,0xd4a6fc88,
 0x6921c1a0,0xbb38007c,0xc9d35faf,0x1bca9e73,0x2c05e009,0xfe1c21d5,0x8cf77e06,0x5eeebfda,
 0xf33819e1,0x2121d83d,0x53ca87ee,0x81d34632,0xb61c3848,0x6405f994,0x16eea647,0xc4f7679b,
 0x79705ab3,0xab699b6f,0xd982c4bc,0x0b9b0560,0x3c547b1a,0xee4dbac6,0x9ca6e515,0x4ebf24c9},

{0x00000000,0x01d8ac87,0x03b1590e,0x0269f589,0x0762b21c,0x06ba1e9b,0x04d3eb12,0x050b4795,
 0x0ec56438,0x0f1dc8bf,0x0d743d36,0x0cac91b1,0x09a7d624,0x087f7aa3,0x0a168f2a,0x0bce23ad,
 0x1d8ac870,0x1c5264f7,0x1e3b917e,0x1fe33df9,0x1ae87a6c,0x1b30d6eb,0x19592362,0x18818fe5,
 0x134fac48,0x129700cf,0x10fef546,0x112659c1,0x142d1e54,0x15f5b2d3,0x179c475a,0x1644ebdd,
 0x3b1590e0,0x3acd3c67,0x38a4c9ee,0x397c6569,0x3c7722fc,0x3daf8e7b,0x3fc67bf2,0x3e1ed775,
 0x35d0f4d8,0x3408585f,0x3661add6,0x37b90151,0x32b246c4,0x336aea43,0x31031fca,0x30dbb34d,
 0x269f5890,0x2747f417,0x252e019e,0x24f6ad19,0x21fdea8c,0x2025460b,0x224cb382,0x23941f05,
 0x285a3ca8,0x2982902f,0x2beb65a6,0x2a33c921,0x2f388eb4,0x2ee02233,0x2c89d7ba,0x2d517b3d,
 0x762b21c0,0x77f38d47,0x759a78ce,0x7442d449,0x714993dc,0x70913f5b,0x72f8cad2,0x73206655,
 0x78ee45f8,0x7936e97f,0x7b5f1cf6,0x7a87b071,0x7f8cf7e4,0x7e545b63,0x7c3daeea,0x7de5026d,
 0x6ba1e9b0,0x6a794537,0x6810b0be,0x69c81c39,0x6cc35bac,0x6d1bf72b,0x6f7202a2,0x6eaaae25,
 0x65648d88,0x64bc210f,0x66d5d486,0x670d7801,0x62063f94,0x63de9313,0x61b7669a,0x606fca1d,
 0x4d3eb120,0x4ce61da7,0x4e8fe82e,0x4f5744a9,0x4a5c033c,0x4b84afbb,0x49ed5a32,0x4835f6b5,
 0x43fbd518,0x4223799f,0x404a8c16,0x41922091,0x44996704,0x4541cb83,0x47283e0a,0x46f0928d,
 0x50b47950,0x516cd5d7,0x5305205e,0x52dd8cd9,0x57d6cb4c,0x560e67cb,0x54679242,0x55bf3ec5,
 0x5e711d68,0x5fa9b1ef,0x5dc04466,0x5c18e8e1,0x5913af74,0x58cb03f3,0x5aa2f67a,0x5b7a5afd,
 0xec564380,0xed8eef07,0xefe71a8e,0xee3fb609,0xeb34f19c,0xeaec5d1b,0xe885a892,0xe95d0415,
 0xe29327b8,0xe34b8b3f,0xe1227eb6,0xe0fad231,0xe5f195a4,0xe4293923,0xe640ccaa,0xe798602d,
 0xf1dc8bf0,0xf0042777,0xf26dd2fe,0xf3b57e79,0xf6be39ec,0xf766956b,0xf50f60e2,0xf4d7cc65,
 0xff19efc8,0xfec1434f,0xfca8b6c6,0xfd701a41,0xf87b5dd4,0xf9a3f153,0xfbca04da,0xfa12a85d,
 0xd743d360,0xd69b7fe7,0xd4f28a6e,0xd52a26e9,0xd021617c,0xd1f9cdfb,0xd3903872,0xd24894f5,
 0xd986b758,0xd85e1bdf,0xda37ee56,0xdbef42d1,0xdee40544,0xdf3ca9c3,0xdd555c4a,0xdc8df0cd,
 0xcac91b10,0xcb11b797,0xc978421e,0xc8a0ee99,0xcdaba90c,0xcc73058b,0xce1af002,0xcfc25c85,
 0xc40c7f28,0xc5d4d3af,0xc7bd2626,0xc6658aa1,0xc36ecd34,0xc2b661b3,0xc0df943a,0xc10738bd,
 0x9a7d6240,0x9ba5cec7,0x99cc3b4e,0x981497c9,0x9d1fd05c,0x9cc77cdb,0x9eae8952,0x9f7625d5,
 0x94b80678,0x9560aaff,0x97095f76,0x96d1f3f1,0x93dab464,0x920218e3,0x906bed6a,0x91b341ed,
 0x87f7aa30,0x862f06b7,0x8446f33e,0x859e5fb9,0x8095182c,0x814db4ab,0x83244122,0x82fceda5,
 0x8932ce08,0x88ea628f,0x8a839706,0x8b5b3b81,0x8e507c14,0x8f88d093,0x8de1251a,0x8c39899d,
 0xa168f2a0,0xa0b05e27,0xa2d9abae,0xa3010729,0xa60a40bc,0xa7d2ec3b,0xa5bb19b2,0xa463b535,
 0xafad9698,0xae753a1f,0xac1ccf96,0xadc46311,0xa8cf2484,0xa9178803,0xab7e7d8a,0xaaa6d10d,
 0xbce23ad0,0xbd3a9657,0xbf5363de,0xbe8bcf59,0xbb8088cc,0xba58244b,0xb831d1c2,0xb9e97d45,
 0xb2275ee8,0xb3fff26f,0xb19607e6,0xb04eab61,0xb545ecf4,0xb49d4073,0xb6f4b5fa,0xb72c197d},

{0x00000000,0xdc6d9ab7,0xbc1a28d9,0x6077b26e,0x7cf54c05,0xa098d6b2,0xc0ef64dc,0x1c82fe6b,
 0xf9ea980a,0x258702bd,0x45f0b0d3,0x999d2a64,0x851fd40f,0x59724eb8,0x3905fcd6,0xe5686661,
 0xf7142da3,0x2b79b714,0x4b0e057a,0x97639fcd,0x8be161a6,0x578cfb11,0x37fb497f,0xeb96d3c8,
 0x0efeb5a9,0xd2932f1e,0xb2e49d70,0x6e8907c7,0x720bf9ac,0xae66631b,0xce11d175,0x127c4bc2,
 0xeae946f1,0x3684dc46,0x56f36e28,0x8a9ef49f,0x961c0af4,0x4a719043,0x2a06222d,0xf66bb89a,
 0x1303defb,0xcf6e444c,0xaf19f622,0x73746c95,0x6ff692fe,0xb39b0849,0xd3ecba27,0x0f812090,
 0x1dfd6b52,0xc190f1e5,0xa1e7438b,0x7d8ad93c,0x61082757,0xbd65bde0,0xdd120f8e,0x017f9539,
 0xe417f358,0x387a69ef,0x580ddb81,0x84604136,0x98e2bf5d,0x448f25ea,0x24f89784,0xf8950d33,
 0xd1139055,0x0d7e0ae2,0x6d09b88c,0xb164223b,0xade6dc50,0x718b46e7,0x11fcf489,0xcd916e3e,
 0x28f9085f,0xf49492e8,0x94e32086,0x488eba31,0x540c445a,0x8861deed,0xe8166c83,0x347bf634,
 0x2607bdf6,0xfa6a2741,0x9a1d952f,0x46700f98,0x5af2f1f3,0x869f6b44,0xe6e8d92a,0x3a85439d,
 0xdfed25fc,0x0380bf4b,0x63f70d25,0xbf9a9792,0xa31869f9,0x7f75f34e,0x1f024120,0xc36fdb97,
 0x3bfad6a4,0xe7974c13,0x87e0fe7d,0x5b8d64ca,0x470f9aa1,0x9b620016,0xfb15b278,0x277828cf,
 0xc2104eae,0x1e7dd419,0x7e0a6677,0xa267fcc0,0xbee502ab,0x6288981c,0x02ff2a72,0xde92b0c5,
 0xcceefb07,0x108361b0,0x70f4d3de,0xac994969,0xb01bb702,0x6c762db5,0x0c019fdb,0xd06c056c,
 0x3504630d,0xe969f9ba,0x891e4bd4,0x5573d163,0x49f12f08,0x959cb5bf,0xf5eb07d1,0x29869d66,
 0xa6e63d1d,0x7a8ba7aa,0x1afc15c4,0xc6918f73,0xda137118,0x067eebaf,0x660959c1,0xba64c376,
 0x5f0ca517,0x83613fa0,0xe3168dce,0x3f7b1779,0x23f9e912,0xff9473a5,0x9fe3c1cb,0x438e5b7c,
 0x51f210be,0x8d9f8a09,0xede83867,0x3185a2d0,0x2d075cbb,0xf16ac60c,0x911d7462,0x4d70eed5,
 0xa81888b4,0x74751203,0x1402a06d,0xc86f3ada,0xd4edc4b1,0x08805e06,0x68f7ec68,0xb49a76df,
 0x4c0f7bec,0x9062e15b,0xf0155335,0x2c78c982,0x30fa37e9,0xec97ad5e,0x8ce01f30,0x508d8587,
 0xb5e5e3e6,0x69887951,0x09ffcb3f,0xd5925188,0xc910afe3,0x157d3554,0x750a873a,0xa9671d8d,
 0xbb1b564f,0x6776ccf8,0x07017e96,0xdb6ce421,0xc7ee1a4a,0x1b8380fd,0x7bf43293,0xa799a824,
 0x42f1ce45,0x9e9c54f2,0xfeebe69c,0x22867c2b,0x3e048240,0xe26918f7,0x821eaa99,0x5e73302e,
 0x77f5ad48,0xab9837ff,0xcbef8591,0x17821f26,0x0b00e14d,0xd76d7bfa,0xb71ac994,0x6b775323,
 0x8e1f3542,0x5272aff5,0x32051d9b,0xee68872c,0xf2ea7947,0x2e87e3f0,0x4ef0519e,0x929dcb29,
 0x80e180eb,0x5c8c1a5c,0x3cfba832,0xe0963285,0xfc14ccee,0x20795659,0x400ee437,0x9c637e80,
 0x790b18e1,0xa5668256,0xc5113038,0x197caa8f,0x05fe54e4,0xd993ce53,0xb9e47c3d,0x6589e68a,
 0x9d1cebb9,0x4171710e,0x2106c360,0xfd6b59d7,0xe1e9a7bc,0x3d843d0b,0x5df38f65,0x819e15d2,
 0x64f673b3,0xb89be904,0xd8ec5b6a,0x0481c1dd,0x18033fb6,0xc46ea501,0xa419176f,0x78748dd8,
 0x6a08c61a,0xb6655cad,0xd612eec3,0x0a7f7474,0x16fd8a1f,0xca9010a8,0xaae7a2c6,0x768a3871,
 0x93e25e10,0x4f8fc4a7,0x2ff876c9,0xf395ec7e,0xef171215,0x337a88a2,0x530d3acc,0x8f60a07b},

{0x00000000,0x490d678d,0x921acf1a,0xdb17a897,0x20f48383,0x69f9e40e,0xb2ee4c99,0xfbe32b14,
 0x41e90706,0x08e4608b,0xd3f3c81c,0x9afeaf91,0x611d8485,0x2810e308,0xf3074b9f,0xba0a2c12,
 0x83d20e0c,0xcadf6981,0x11c8c116,0x58c5a69b,0xa3268d8f,0xea2bea02,0x313c4295,0x78312518,
 0xc23b090a,0x8b366e87,0x5021c610,0x192ca19d,0xe2cf8a89,0xabc2ed04,0x70d54593,0x39d8221e,
 0x036501af,0x4a686622,0x917fceb5,0xd872a938,0x2391822c,0x6a9ce5a1,0xb18b4d36,0xf8862abb,
 0x428c06a9,0x0b816124,0xd096c9b3,0x999bae3e,0x6278852a,0x2b75e2a7,0xf0624a30,0xb96f2dbd,
 0x80b70fa3,0xc9ba682e,0x12adc0b9,0x5ba0a734,0xa0438c20,0xe94eebad,0x3259433a,0x7b5424b7,
 0xc15e08a5,0x88536f28,0x5344c7bf,0x1a49a032,0xe1aa8b26,0xa8a7ecab,0x73b0443c,0x3abd23b1,
 0x06ca035e,0x4fc764d3,0x94d0cc44,0xddddabc9,0x263e80dd,0x6f33e750,0xb4244fc7,0xfd29284a,
 0x47230458,0x0e2e63d5,0xd539cb42,0x9c34accf,0x67d787db,0x2edae056,0xf5cd48c1,0xbcc02f4c,
 0x85180d52,0xcc156adf,0x1702c248,0x5e0fa5c5,0xa5ec8ed1,0xece1e95c,0x37f641cb,0x7efb2646,
 0xc4f10a54,0x8dfc6dd9,0x56ebc54e,0x1fe6a2c3,0xe40589d7,0xad08ee5a,0x761f46cd,0x3f122140,
 0x05af02f1,0x4ca2657c,0x97b5cdeb,0xdeb8aa66,0x255b8172,0x6c56e6ff,0xb7414e68,0xfe4c29e5,
 0x444605f7,0x0d4b627a,0xd65ccaed,0x9f51ad60,0x64b28674,0x2dbfe1f9,0xf6a8496e,0xbfa52ee3,
 0x867d0cfd,0xcf706b70,0x1467c3e7,0x5d6aa46a,0xa6898f7e,0xef84e8f3,0x34934064,0x7d9e27e9,
 0xc7940bfb,0x8e996c76,0x558ec4e1,0x1c83a36c,0xe7608878,0xae6deff5,0x757a4762,0x3c7720ef,
 0x0d9406bc,0x44996131,0x9f8ec9a6,0xd683ae2b,0x2d60853f,0x646de2b2,0xbf7a4a25,0xf6772da8,
 0x4c7d01ba,0x05706637,0xde67cea0,0x976aa92d,0x6c898239,0x2584e5b4,0xfe934d23,0xb79e2aae,
 0x8e4608b0,0xc74b6f3d,0x1c5cc7aa,0x5551a027,0xaeb28b33,0xe7bfecbe,0x3ca84429,0x75a523a4,
 0xcfaf0fb6,0x86a2683b,0x5db5c0ac,0x14b8a721,0xef5b8c35,0xa656ebb8,0x7d41432f,0x344c24a2,
 0x0ef10713,0x47fc609e,0x9cebc809,0xd5e6af84,0x2e058490,0x6708e31d,0xbc1f4b8a,0xf5122c07,
 0x4f180015,0x06156798,0xdd02cf0f,0x940fa882,0x6fec8396,0x26e1e41b,0xfdf64c8c,0xb4fb2b01,
 0x8d23091f,0xc42e6e92,0x1f39c605,0x5634a188,0xadd78a9c,0xe4daed11,0x3fcd4586,0x76c0220b,
 0xccca0e19,0x85c76994,0x5ed0c103,0x17dda68e,0xec3e8d9a,0xa533ea17,0x7e244280,0x3729250d,
 0x0b5e05e2,0x4253626f,0x9944caf8,0xd049ad75,0x2baa8661,0x62a7e1ec,0xb9b0497b,0xf0bd2ef6,
 0x4ab702e4,0x03ba6569,0xd8adcdfe,0x91a0aa73,0x6a438167,0x234ee6ea,0xf8594e7d,0xb15429f0,
 0x888c0bee,0xc1816c63,0x1a96c4f4,0x539ba379,0xa878886d,0xe175efe0,0x3a624777,0x736f20fa,
 0xc9650ce8,0x80686b65,0x5b7fc3f2,0x1272a47f,0xe9918f6b,0xa09ce8e6,0x7b8b4071,0x328627fc,
 0x083b044d,0x413663c0,0x9a21cb57,0xd32cacda,0x28cf87ce,0x61c2e043,0xbad548d4,0xf3d82f59,
 0x49d2034b,0x00df64c6,0xdbc8cc51,0x92c5abdc,0x692680c8,0x202be745,0xfb3c4fd2,0xb231285f,
 0x8be90a41,0xc2e46dcc,0x19f3c55b,0x50fea2d6,0xab1d89c2,0xe210ee4f,0x390746d8,0x700a2155,
 0xca000d47,0x830d6aca,0x581ac25d,0x1117a5d0,0xeaf48ec4,0xa3f9e949,0x78ee41de,0x31e32653},

{0x00000000,0x1b280d78,0x36501af0,0x2d781788,0x6ca035e0,0x77883898,0x5af02f10,0x41d82268,
 0xd9406bc0,0xc26866b8,0xef107130,0xf4387c48,0xb5e05e20,0xaec85358,0x83b044d0,0x989849a8,
 0xb641ca37,0xad69c74f,0x8011d0c7,0x9b39ddbf,0xdae1ffd7,0xc1c9f2af,0xecb1e527,0xf799e85f,
 0x6f01a1f7,0x7429ac8f,0x5951bb07,0x4279b67f,0x03a19417,0x1889996f,0x35f18ee7,0x2ed9839f,
 0x684289d9,0x736a84a1,0x5e129329,0x453a9e51,0x04e2bc39,0x1fcab141,0x32b2a6c9,0x299aabb1,
 0xb102e219,0xaa2aef61,0x8752f8e9,0x9c7af591,0xdda2d7f9,0xc68ada81,0xebf2cd09,0xf0dac071,
 0xde0343ee,0xc52b4e96,0xe853591e,0xf37b5466,0xb2a3760e,0xa98b7b76,0x84f36cfe,0x9fdb6186,
 0x0743282e,0x1c6b2556,0x311332de,0x2a3b3fa6,0x6be31dce,0x70cb10b6,0x5db3073e,0x469b0a46,
 0xd08513b2,0xcbad1eca,0xe6d50942,0xfdfd043a,0xbc252652,0xa70d2b2a,0x8a753ca2,0x915d31da,
 0x09c57872,0x12ed750a,0x3f956282,0x24bd6ffa,0x65654d92,0x7e4d40ea,0x53355762,0x481d5a1a,
 0x66c4d985,0x7decd4fd,0x5094c375,0x4bbcce0d,0x0a64ec65,0x114ce11d,0x3c34f695,0x271cfbed,
 0xbf84b245,0xa4acbf3d,0x89d4a8b5,0x92fca5cd,0xd32487a5,0xc80c8add,0xe5749d55,0xfe5c902d,
 0xb8c79a6b,0xa3ef9713,0x8e97809b,0x95bf8de3,0xd467af8b,0xcf4fa2f3,0xe237b57b,0xf91fb803,
 0x6187f1ab,0x7aaffcd3,0x57d7eb5b,0x4cffe623,0x0d27c44b,0x160fc933,0x3b77debb,0x205fd3c3,
 0x0e86505c,0x15ae5d24,0x38d64aac,0x23fe47d4,0x622665bc,0x790e68c4,0x54767f4c,0x4f5e7234,
 0xd7c63b9c,0xccee36e4,0xe196216c,0xfabe2c14,0xbb660e7c,0xa04e0304,0x8d36148c,0x961e19f4,
 0xa5cb3ad3,0xbee337ab,0x939b2023,0x88b32d5b,0xc96b0f33,0xd243024b,0xff3b15c3,0xe41318bb,
 0x7c8b5113,0x67a35c6b,0x4adb4be3,0x51f3469b,0x102b64f3,0x0b03698b,0x267b7e03,0x3d53737b,
 0x138af0e4,0x08a2fd9c,0x25daea14,0x3ef2e76c,0x7f2ac504,0x6402c87c,0x497adff4,0x5252d28c,
 0xcaca9b24,0xd1e2965c,0xfc9a81d4,0xe7b28cac,0xa66aaec4,0xbd42a3bc,0x903ab434,0x8b12b94c,
 0xcd89b30a,0xd6a1be72,0xfbd9a9fa,0xe0f1a482,0xa12986ea,0xba018b92,0x97799c1a,0x8c519162,
 0x14c9d8ca,0x0fe1d5b2,0x2299c23a,0x39b1cf42,0x7869ed2a,0x6341e052,0x4e39f7da,0x5511faa2,
 0x7bc8793d,0x60e07445,0x4d9863cd,0x56b06eb5,0x17684cdd,0x0c4041a5,0x2138562d,0x3a105b55,
 0xa28812fd,0xb9a01f85,0x94d8080d,0x8ff00575,0xce28271d,0xd5002a65,0xf8783ded,0xe3503095,
 0x754e2961,0x6e662419,0x431e3391,0x58363ee9,0x19ee1c81,0x02c611f9,0x2fbe0671,0x34960b09,
 0xac0e42a1,0xb7264fd9,0x9a5e5851,0x81765529,0xc0ae7741,0xdb867a39,0xf6fe6db1,0xedd660c9,
 0xc30fe356,0xd827ee2e,0xf55ff9a6,0xee77f4de,0xafafd6b6,0xb487dbce,0x99ffcc46,0x82d7c13e,
 0x1a4f8896,0x016785ee,0x2c1f9266,0x37379f1e,0x76efbd76,0x6dc7b00e,0x40bfa786,0x5b97aafe,
 0x1d0ca0b8,0x0624adc0,0x2b5cba48,0x3074b730,0x71ac9558,0x6a849820,0x47fc8fa8,0x5cd482d0,
 0xc44ccb78,0xdf64c600,0xf21cd188,0xe934dcf0,0xa8ecfe98,0xb3c4f3e0,0x9ebce468,0x8594e910,
 0xab4d6a8f,0xb06567f7,0x9d1d707f,0x86357d07,0xc7ed5f6f,0xdcc55217,0xf1bd459f,0xea9548e7,
 0x720d014f,0x69250c37,0x445d1bbf,0x5f7516c7,0x1ead34af,0x058539d7,0x28fd2e5f,0x33d52327},

{0x00000000,0x4f576811,0x9eaed022,0xd1f9b833,0x399cbdf3,0x76cbd5e2,0xa7326dd1,0xe86505c0,
 0x73397be6,0x3c6e13f7,0xed97abc4,0xa2c0c3d5,0x4aa5c615,0x05f2ae04,0xd40b1637,0x9b5c7e26,
 0xe672f7cc,0xa9259fdd,0x78dc27ee,0x378b4fff,0xdfee4a3f,0x90b9222e,0x41409a1d,0x0e17f20c,
 0x954b8c2a,0xda1ce43b,0x0be55c08,0x44b23419,0xacd731d9,0xe38059c8,0x3279e1fb,0x7d2e89ea,
 0xc824f22f,0x87739a3e,0x568a220d,0x19dd4a1c,0xf1b84fdc,0xbeef27cd,0x6f169ffe,0x2041f7ef,
 0xbb1d89c9,0xf44ae1d8,0x25b359eb,0x6ae431fa,0x8281343a,0xcdd65c2b,0x1c2fe418,0x53788c09,
 0x2e5605e3,0x61016df2,0xb0f8d5c1,0xffafbdd0,0x17cab810,0x589dd001,0x89646832,0xc6330023,
 0x5d6f7e05,0x12381614,0xc3c1ae27,0x8c96c636,0x64f3c3f6,0x2ba4abe7,0xfa5d13d4,0xb50a7bc5,
 0x9488f9e9,0xdbdf91f8,0x0a2629cb,0x457141da,0xad14441a,0xe2432c0b,0x33ba9438,0x7cedfc29,
 0xe7b1820f,0xa8e6ea1e,0x791f522d,0x36483a3c,0xde2d3ffc,0x917a57ed,0x4083efde,0x0fd487cf,
 0x72fa0e25,0x3dad6634,0xec54de07,0xa303b616,0x4b66b3d6,0x0431dbc7,0xd5c863f4,0x9a9f0be5,
 0x01c375c3,0x4e941dd2,0x9f6da5e1,0xd03acdf0,0x385fc830,0x7708a021,0xa6f11812,0xe9a67003,
 0x5cac0bc6,0x13fb63d7,0xc202dbe4,0x8d55b3f5,0x6530b635,0x2a67de24,0xfb9e6617,0xb4c90e06,
 0x2f957020,0x60c21831,0xb13ba002,0xfe6cc813,0x1609cdd3,0x595ea5c2,0x88a71df1,0xc7f075e0,
 0xbadefc0a,0xf589941b,0x24702c28,0x6b274439,0x834241f9,0xcc1529e8,0x1dec91db,0x52bbf9ca,
 0xc9e787ec,0x86b0effd,0x574957ce,0x181e3fdf,0xf07b3a1f,0xbf2c520e,0x6ed5ea3d,0x2182822c,
 0x2dd0ee65,0x62878674,0xb37e3e47,0xfc295656,0x144c5396,0x5b1b3b87,0x8ae283b4,0xc5b5eba5,
 0x5ee99583,0x11befd92,0xc04745a1,0x8f102db0,0x67752870,0x28224061,0xf9dbf852,0xb68c9043,
 0xcba219a9,0x84f571b8,0x550cc98b,0x1a5ba19a,0xf23ea45a,0xbd69cc4b,0x6c907478,0x23c71c69,
 0xb89b624f,0xf7cc0a5e,0x2635b26d,0x6962da7c,0x8107dfbc,0xce50b7ad,0x1fa90f9e,0x50fe678f,
 0xe5f41c4a,0xaaa3745b,0x7b5acc68,0x340da479,0xdc68a1b9,0x933fc9a8,0x42c6719b,0x0d91198a,
 0x96cd67ac,0xd99a0fbd,0x0863b78e,0x4734df9f,0xaf51da5f,0xe006b24e,0x31ff0a7d,0x7ea8626c,
 0x0386eb86,0x4cd18397,0x9d283ba4,0xd27f53b5,0x3a1a5675,0x754d3e64,0xa4b48657,0xebe3ee46,
 0x70bf9060,0x3fe8f871,0xee114042,0xa1462853,0x49232d93,0x06744582,0xd78dfdb1,0x98da95a0,
 0xb958178c,0xf60f7f9d,0x27f6c7ae,0x68a1afbf,0x80c4aa7f,0xcf93c26e,0x1e6a7a5d,0x513d124c,
 0xca616c6a,0x8536047b,0x54cfbc48,0x1b98d459,0xf3fdd199,0xbcaab988,0x6d5301bb,0x220469aa,
 0x5f2ae040,0x107d8851,0xc1843062,0x8ed35873,0x66b65db3,0x29e135a2,0xf8188d91,0xb74fe580,
 0x2c139ba6,0x6344f3b7,0xb2bd4b84,0xfdea2395,0x158f2655,0x5ad84e44,0x8b21f677,0xc4769e66,
 0x717ce5a3,0x3e2b8db2,0xefd23581,0xa0855d90,0x48e05850,0x07b73041,0xd64e8872,0x9919e063,
 0x02459e45,0x4d12f654,0x9ceb4e67,0xd3bc2676,0x3bd923b6,0x748e4ba7,0xa577f394,0xea209b85,
 0x970e126f,0xd8597a7e,0x09a0c24d,0x46f7aa5c,0xae92af9c,0xe1c5c78d,0x303c7fbe,0x7f6b17af,
 0xe4376989,0xab600198,0x7a99b9ab,0x35ced1ba,0xddabd47a,0x92fcbc6b,0x43050458,0x0c526c49},

{0x00000000,0x5ba1dcca,0xb743b994,0xece2655e,0x6a466e9f,0x31e7b255,0xdd05d70b,0x86a40bc1,
 0xd48cdd3e,0x8f2d01f4,0x63cf64aa,0x386eb860,0xbecab3a1,0xe56b6f6b,0x09890a35,0x5228d6ff,
 0xadd8a7cb,0xf6797b01,0x1a9b1e5f,0x413ac295,0xc79ec954,0x9c3f159e,0x70dd70c0,0x2b7cac0a,
 0x79547af5,0x22f5a63f,0xce17c361,0x95b61fab,0x1312146a,0x48b3c8a0,0xa451adfe,0xfff07134,
 0x5f705221,0x04d18eeb,0xe833ebb5,0xb392377f,0x35363cbe,0x6e97e074,0x8275852a,0xd9d459e0,
 0x8bfc8f1f,0xd05d53d5,0x3cbf368b,0x671eea41,0xe1bae180,0xba1b3d4a,0x56f95814,0x0d5884de,
 0xf2a8f5ea,0xa9092920,0x45eb4c7e,0x1e4a90b4,0x98ee9b75,0xc34f47bf,0x2fad22e1,0x740cfe2b,
 0x262428d4,0x7d85f41e,0x91679140,0xcac64d8a,0x4c62464b,0x17c39a81,0xfb21ffdf,0xa0802315,
 0xbee0a442,0xe5417888,0x09a31dd6,0x5202c11c,0xd4a6cadd,0x8f071617,0x63e57349,0x3844af83,
 0x6a6c797c,0x31cda5b6,0xdd2fc0e8,0x868e1c22,0x002a17e3,0x5b8bcb29,0xb769ae77,0xecc872bd,
 0x13380389,0x4899df43,0xa47bba1d,0xffda66d7,0x797e6d16,0x22dfb1dc,0xce3dd482,0x959c0848,
 0xc7b4deb7,0x9c15027d,0x70f76723,0x2b56bbe9,0xadf2b028,0xf6536ce2,0x1ab109bc,0x4110d576,
 0xe190f663,0xba312aa9,0x56d34ff7,0x0d72933d,0x8bd698fc,0xd0774436,0x3c952168,0x6734fda2,
 0x351c2b5d,0x6ebdf797,0x825f92c9,0xd9fe4e03,0x5f5a45c2,0x04fb9908,0xe819fc56,0xb3b8209c,
 0x4c4851a8,0x17e98d62,0xfb0be83c,0xa0aa34f6,0x260e3f37,0x7dafe3fd,0x914d86a3,0xcaec5a69,
 0x98c48c96,0xc365505c,0x2f873502,0x7426e9c8,0xf282e209,0xa9233ec3,0x45c15b9d,0x1e608757,
 0x79005533,0x22a189f9,0xce43eca7,0x95e2306d,0x13463bac,0x48e7e766,0xa4058238,0xffa45ef2,
 0xad8c880d,0xf62d54c7,0x1acf3199,0x416eed53,0xc7cae692,0x9c6b3a58,0x70895f06,0x2b2883cc,
 0xd4d8f2f8,0x8f792e32,0x639b4b6c,0x383a97a6,0xbe9e9c67,0xe53f40ad,0x09dd25f3,0x527cf939,
 0x00542fc6,0x5bf5f30c,0xb7179652,0xecb64a98,0x6a124159,0x31b39d93,0xdd51f8cd,0x86f02407,
 0x26700712,0x7dd1dbd8,0x9133be86,0xca92624c,0x4c36698d,0x1797b547,0xfb75d019,0xa0d40cd3,
 0xf2fcda2c,0xa95d06e6,0x45bf63b8,0x1e1ebf72,0x98bab4b3,0xc31b6879,0x2ff90d27,0x7458d1ed,
 0x8ba8a0d9,0xd0097c13,0x3ceb194d,0x674ac587,0xe1eece46,0xba4f128c,0x56ad77d2,0x0d0cab18,
 0x5f247de7,0x0485a12d,0xe867c473,0xb3c618b9,0x35621378,0x6ec3cfb2,0x8221aaec,0xd9807626,
 0xc7e0f171,0x9c412dbb,0x70a348e5,0x2b02942f,0xada69fee,0xf6074324,0x1ae5267a,0x4144fab0,
 0x136c2c4f,0x48cdf085,0xa42f95db,0xff8e4911,0x792a42d0,0x228b9e1a,0xce69fb44,0x95c8278e,
 0x6a3856ba,0x31998a70,0xdd7bef2e,0x86da33e4,0x007e3825,0x5bdfe4ef,0xb73d81b1,0xec9c5d7b,
 0xbeb48b84,0xe515574e,0x09f73210,0x5256eeda,0xd4f2e51b,0x8f5339d1,0x63b15c8f,0x38108045,
 0x9890a350,0xc3317f9a,0x2fd31ac4,0x7472c60e,0xf2d6cdcf,0xa9771105,0x4595745b,0x1e34a891,
 0x4c1c7e6e,0x17bda2a4,0xfb5fc7fa,0xa0fe1b30,0x265a10f1,0x7dfbcc3b,0x9119a965,0xcab875af,
 0x3548049b,0x6ee9d851,0x820bbd0f,0xd9aa61c5,0x5f0e6a04,0x04afb6ce,0xe84dd390,0xb3ec0f5a,
 0xe1c4d9a5,0xba65056f,0x56876031,0x0d26bcfb,0x8b82b73a,0xd0236bf0,0x3cc10eae,0x6760d264}};

/* init the encode/decode logical stream state */

int ogg_stream_init(ogg_stream_state *os,int serialno){
  if(os){
    memset(os,0,sizeof(*os));
    os->body_storage=16*1024;
    os->lacing_storage=1024;

    os->body_data=(unsigned char*)_ogg_malloc(os->body_storage*sizeof(*os->body_data));
    os->lacing_vals=(int*)_ogg_malloc(os->lacing_storage*sizeof(*os->lacing_vals));
    os->granule_vals=(ogg_int64_t*)_ogg_malloc(os->lacing_storage*sizeof(*os->granule_vals));

    if(!os->body_data || !os->lacing_vals || !os->granule_vals){
      ogg_stream_clear(os);
      return -1;
    }

    os->serialno=serialno;

    return(0);
  }
  return(-1);
}

/* async/delayed error detection for the ogg_stream_state */
int ogg_stream_check(ogg_stream_state *os){
  if(!os || !os->body_data) return -1;
  return 0;
}

/* _clear does not free os, only the non-flat storage within */
int ogg_stream_clear(ogg_stream_state *os){
  if(os){
    if(os->body_data)_ogg_free(os->body_data);
    if(os->lacing_vals)_ogg_free(os->lacing_vals);
    if(os->granule_vals)_ogg_free(os->granule_vals);

    memset(os,0,sizeof(*os));
  }
  return(0);
}

int ogg_stream_destroy(ogg_stream_state *os){
  if(os){
    ogg_stream_clear(os);
    _ogg_free(os);
  }
  return(0);
}

/* Helpers for ogg_stream_encode; this keeps the structure and
   what's happening fairly clear */

static int _os_body_expand(ogg_stream_state *os,long needed){
  if(os->body_storage-needed<=os->body_fill){
    long body_storage;
    void *ret;
    if(os->body_storage>LONG_MAX-needed){
      ogg_stream_clear(os);
      return -1;
    }
    body_storage=os->body_storage+needed;
    if(body_storage<LONG_MAX-1024)body_storage+=1024;
    ret=_ogg_realloc(os->body_data,body_storage*sizeof(*os->body_data));
    if(!ret){
      ogg_stream_clear(os);
      return -1;
    }
    os->body_storage=body_storage;
    os->body_data=(unsigned char*)ret;
  }
  return 0;
}

static int _os_lacing_expand(ogg_stream_state *os,long needed){
  if(os->lacing_storage-needed<=os->lacing_fill){
    long lacing_storage;
    void *ret;
    if(os->lacing_storage>LONG_MAX-needed){
      ogg_stream_clear(os);
      return -1;
    }
    lacing_storage=os->lacing_storage+needed;
    if(lacing_storage<LONG_MAX-32)lacing_storage+=32;
    ret=_ogg_realloc(os->lacing_vals,lacing_storage*sizeof(*os->lacing_vals));
    if(!ret){
      ogg_stream_clear(os);
      return -1;
    }
    os->lacing_vals=(int*)ret;
    ret=_ogg_realloc(os->granule_vals,lacing_storage*
                     sizeof(*os->granule_vals));
    if(!ret){
      ogg_stream_clear(os);
      return -1;
    }
    os->granule_vals=(ogg_int64_t*)ret;
    os->lacing_storage=lacing_storage;
  }
  return 0;
}

/* checksum the page */
/* Direct table CRC; note that this will be faster in the future if we
   perform the checksum simultaneously with other copies */

static ogg_uint32_t _os_update_crc(ogg_uint32_t crc, unsigned char *buffer, int size){
  while (size>=8){
    crc^=((ogg_uint32_t)buffer[0]<<24)|((ogg_uint32_t)buffer[1]<<16)|((ogg_uint32_t)buffer[2]<<8)|((ogg_uint32_t)buffer[3]);

    crc=crc_lookup[7][ crc>>24      ]^crc_lookup[6][(crc>>16)&0xFF]^
        crc_lookup[5][(crc>> 8)&0xFF]^crc_lookup[4][ crc     &0xFF]^
        crc_lookup[3][buffer[4]     ]^crc_lookup[2][buffer[5]     ]^
        crc_lookup[1][buffer[6]     ]^crc_lookup[0][buffer[7]     ];

    buffer+=8;
    size-=8;
  }

  while (size--)
    crc=(crc<<8)^crc_lookup[0][((crc >> 24)&0xff)^*buffer++];
  return crc;
}

void ogg_page_checksum_set(ogg_page *og){
  if(og){
    ogg_uint32_t crc_reg=0;

    /* safety; needed for API behavior, but not framing code */
    og->header[22]=0;
    og->header[23]=0;
    og->header[24]=0;
    og->header[25]=0;

    crc_reg=_os_update_crc(crc_reg,og->header,og->header_len);
    crc_reg=_os_update_crc(crc_reg,og->body,og->body_len);

    og->header[22]=(unsigned char)(crc_reg&0xff);
    og->header[23]=(unsigned char)((crc_reg>>8)&0xff);
    og->header[24]=(unsigned char)((crc_reg>>16)&0xff);
    og->header[25]=(unsigned char)((crc_reg>>24)&0xff);
  }
}

/* submit data to the internal buffer of the framing engine */
int ogg_stream_iovecin(ogg_stream_state *os, ogg_iovec_t *iov, int count,
                       long e_o_s, ogg_int64_t granulepos){

  long bytes = 0, lacing_vals;
  int i;

  if(ogg_stream_check(os)) return -1;
  if(!iov) return 0;

  for (i = 0; i < count; ++i){
    if(iov[i].iov_len>LONG_MAX) return -1;
    if(bytes>LONG_MAX-(long)iov[i].iov_len) return -1;
    bytes += (long)iov[i].iov_len;
  }
  lacing_vals=bytes/255+1;

  if(os->body_returned){
    /* advance packet data according to the body_returned pointer. We
       had to keep it around to return a pointer into the buffer last
       call */

    os->body_fill-=os->body_returned;
    if(os->body_fill)
      memmove(os->body_data,os->body_data+os->body_returned,
              os->body_fill);
    os->body_returned=0;
  }

  /* make sure we have the buffer storage */
  if(_os_body_expand(os,bytes) || _os_lacing_expand(os,lacing_vals))
    return -1;

  /* Copy in the submitted packet.  Yes, the copy is a waste; this is
     the liability of overly clean abstraction for the time being.  It
     will actually be fairly easy to eliminate the extra copy in the
     future */

  for (i = 0; i < count; ++i) {
    memcpy(os->body_data+os->body_fill, iov[i].iov_base, iov[i].iov_len);
    os->body_fill += (int)iov[i].iov_len;
  }

  /* Store lacing vals for this packet */
  for(i=0;i<lacing_vals-1;i++){
    os->lacing_vals[os->lacing_fill+i]=255;
    os->granule_vals[os->lacing_fill+i]=os->granulepos;
  }
  os->lacing_vals[os->lacing_fill+i]=bytes%255;
  os->granulepos=os->granule_vals[os->lacing_fill+i]=granulepos;

  /* flag the first segment as the beginning of the packet */
  os->lacing_vals[os->lacing_fill]|= 0x100;

  os->lacing_fill+=lacing_vals;

  /* for the sake of completeness */
  os->packetno++;

  if(e_o_s)os->e_o_s=1;

  return(0);
}

int ogg_stream_packetin(ogg_stream_state *os,ogg_packet *op){
  ogg_iovec_t iov;
  iov.iov_base = op->packet;
  iov.iov_len = op->bytes;
  return ogg_stream_iovecin(os, &iov, 1, op->e_o_s, op->granulepos);
}

/* Conditionally flush a page; force==0 will only flush nominal-size
   pages, force==1 forces us to flush a page regardless of page size
   so long as there's any data available at all. */
static int ogg_stream_flush_i(ogg_stream_state *os,ogg_page *og, int force, int nfill){
  int i;
  int vals=0;
  int maxvals=(os->lacing_fill>255?255:os->lacing_fill);
  int bytes=0;
  long acc=0;
  ogg_int64_t granule_pos=-1;

  if(ogg_stream_check(os)) return(0);
  if(maxvals==0) return(0);

  /* construct a page */
  /* decide how many segments to include */

  /* If this is the initial header case, the first page must only include
     the initial header packet */
  if(os->b_o_s==0){  /* 'initial header page' case */
    granule_pos=0;
    for(vals=0;vals<maxvals;vals++){
      if((os->lacing_vals[vals]&0x0ff)<255){
        vals++;
        break;
      }
    }
  }else{

    /* The extra packets_done, packet_just_done logic here attempts to do two things:
       1) Don't unnecessarily span pages.
       2) Unless necessary, don't flush pages if there are less than four packets on
          them; this expands page size to reduce unnecessary overhead if incoming packets
          are large.
       These are not necessary behaviors, just 'always better than naive flushing'
       without requiring an application to explicitly request a specific optimized
       behavior. We'll want an explicit behavior setup pathway eventually as well. */

    int packets_done=0;
    int packet_just_done=0;
    for(vals=0;vals<maxvals;vals++){
      if(acc>nfill && packet_just_done>=4){
        force=1;
        break;
      }
      acc+=os->lacing_vals[vals]&0x0ff;
      if((os->lacing_vals[vals]&0xff)<255){
        granule_pos=os->granule_vals[vals];
        packet_just_done=++packets_done;
      }else
        packet_just_done=0;
    }
    if(vals==255)force=1;
  }

  if(!force) return(0);

  /* construct the header in temp storage */
  memcpy(os->header,"OggS",4);

  /* stream structure version */
  os->header[4]=0x00;

  /* continued packet flag? */
  os->header[5]=0x00;
  if((os->lacing_vals[0]&0x100)==0)os->header[5]|=0x01;
  /* first page flag? */
  if(os->b_o_s==0)os->header[5]|=0x02;
  /* last page flag? */
  if(os->e_o_s && os->lacing_fill==vals)os->header[5]|=0x04;
  os->b_o_s=1;

  /* 64 bits of PCM position */
  for(i=6;i<14;i++){
    os->header[i]=(unsigned char)(granule_pos&0xff);
    granule_pos>>=8;
  }

  /* 32 bits of stream serial number */
  {
    long serialno=os->serialno;
    for(i=14;i<18;i++){
      os->header[i]=(unsigned char)(serialno&0xff);
      serialno>>=8;
    }
  }

  /* 32 bits of page counter (we have both counter and page header
     because this val can roll over) */
  if(os->pageno==-1)os->pageno=0; /* because someone called
                                     stream_reset; this would be a
                                     strange thing to do in an
                                     encode stream, but it has
                                     plausible uses */
  {
    long pageno=os->pageno++;
    for(i=18;i<22;i++){
      os->header[i]=(unsigned char)(pageno&0xff);
      pageno>>=8;
    }
  }

  /* zero for computation; filled in later */
  os->header[22]=0;
  os->header[23]=0;
  os->header[24]=0;
  os->header[25]=0;

  /* segment table */
  os->header[26]=(unsigned char)(vals&0xff);
  for(i=0;i<vals;i++)
    bytes+=os->header[i+27]=(unsigned char)(os->lacing_vals[i]&0xff);

  /* set pointers in the ogg_page struct */
  og->header=os->header;
  og->header_len=os->header_fill=vals+27;
  og->body=os->body_data+os->body_returned;
  og->body_len=bytes;

  /* advance the lacing data and set the body_returned pointer */

  os->lacing_fill-=vals;
  memmove(os->lacing_vals,os->lacing_vals+vals,os->lacing_fill*sizeof(*os->lacing_vals));
  memmove(os->granule_vals,os->granule_vals+vals,os->lacing_fill*sizeof(*os->granule_vals));
  os->body_returned+=bytes;

  /* calculate the checksum */

  ogg_page_checksum_set(og);

  /* done */
  return(1);
}

/* This will flush remaining packets into a page (returning nonzero),
   even if there is not enough data to trigger a flush normally
   (undersized page). If there are no packets or partial packets to
   flush, ogg_stream_flush returns 0.  Note that ogg_stream_flush will
   try to flush a normal sized page like ogg_stream_pageout; a call to
   ogg_stream_flush does not guarantee that all packets have flushed.
   Only a return value of 0 from ogg_stream_flush indicates all packet
   data is flushed into pages.

   since ogg_stream_flush will flush the last page in a stream even if
   it's undersized, you almost certainly want to use ogg_stream_pageout
   (and *not* ogg_stream_flush) unless you specifically need to flush
   a page regardless of size in the middle of a stream. */

int ogg_stream_flush(ogg_stream_state *os,ogg_page *og){
  return ogg_stream_flush_i(os,og,1,4096);
}

/* Like the above, but an argument is provided to adjust the nominal
   page size for applications which are smart enough to provide their
   own delay based flushing */

int ogg_stream_flush_fill(ogg_stream_state *os,ogg_page *og, int nfill){
  return ogg_stream_flush_i(os,og,1,nfill);
}

/* This constructs pages from buffered packet segments.  The pointers
returned are to static buffers; do not free. The returned buffers are
good only until the next call (using the same ogg_stream_state) */

int ogg_stream_pageout(ogg_stream_state *os, ogg_page *og){
  int force=0;
  if(ogg_stream_check(os)) return 0;

  if((os->e_o_s&&os->lacing_fill) ||          /* 'were done, now flush' case */
     (os->lacing_fill&&!os->b_o_s))           /* 'initial header page' case */
    force=1;

  return(ogg_stream_flush_i(os,og,force,4096));
}

/* Like the above, but an argument is provided to adjust the nominal
page size for applications which are smart enough to provide their
own delay based flushing */

int ogg_stream_pageout_fill(ogg_stream_state *os, ogg_page *og, int nfill){
  int force=0;
  if(ogg_stream_check(os)) return 0;

  if((os->e_o_s&&os->lacing_fill) ||          /* 'were done, now flush' case */
     (os->lacing_fill&&!os->b_o_s))           /* 'initial header page' case */
    force=1;

  return(ogg_stream_flush_i(os,og,force,nfill));
}

int ogg_stream_eos(ogg_stream_state *os){
  if(ogg_stream_check(os)) return 1;
  return os->e_o_s;
}

/* DECODING PRIMITIVES: packet streaming layer **********************/

/* This has two layers to place more of the multi-serialno and paging
   control in the application's hands.  First, we expose a data buffer
   using ogg_sync_buffer().  The app either copies into the
   buffer, or passes it directly to read(), etc.  We then call
   ogg_sync_wrote() to tell how many bytes we just added.

   Pages are returned (pointers into the buffer in ogg_sync_state)
   by ogg_sync_pageout().  The page is then submitted to
   ogg_stream_pagein() along with the appropriate
   ogg_stream_state* (ie, matching serialno).  We then get raw
   packets out calling ogg_stream_packetout() with a
   ogg_stream_state. */

/* initialize the struct to a known state */
int ogg_sync_init(ogg_sync_state *oy){
  if(oy){
    oy->storage = -1; /* used as a readiness flag */
    memset(oy,0,sizeof(*oy));
  }
  return(0);
}

/* clear non-flat storage within */
int ogg_sync_clear(ogg_sync_state *oy){
  if(oy){
    if(oy->data)_ogg_free(oy->data);
    memset(oy,0,sizeof(*oy));
  }
  return(0);
}

int ogg_sync_destroy(ogg_sync_state *oy){
  if(oy){
    ogg_sync_clear(oy);
    _ogg_free(oy);
  }
  return(0);
}

int ogg_sync_check(ogg_sync_state *oy){
  if(oy->storage<0) return -1;
  return 0;
}

char *ogg_sync_buffer(ogg_sync_state *oy, long size){
  if(ogg_sync_check(oy)) return NULL;

  /* first, clear out any space that has been previously returned */
  if(oy->returned){
    oy->fill-=oy->returned;
    if(oy->fill>0)
      memmove(oy->data,oy->data+oy->returned,oy->fill);
    oy->returned=0;
  }

  if(size>oy->storage-oy->fill){
    /* We need to extend the internal buffer */
    long newsize=size+oy->fill+4096; /* an extra page to be nice */
    void *ret;

    if(oy->data)
      ret=_ogg_realloc(oy->data,newsize);
    else
      ret=_ogg_malloc(newsize);
    if(!ret){
      ogg_sync_clear(oy);
      return NULL;
    }
    oy->data=(unsigned char*)ret;
    oy->storage=newsize;
  }

  /* expose a segment at least as large as requested at the fill mark */
  return((char *)oy->data+oy->fill);
}

int ogg_sync_wrote(ogg_sync_state *oy, long bytes){
  if(ogg_sync_check(oy))return -1;
  if(oy->fill+bytes>oy->storage)return -1;
  oy->fill+=bytes;
  return(0);
}

/* sync the stream.  This is meant to be useful for finding page
   boundaries.

   return values for this:
  -n) skipped n bytes
   0) page not ready; more data (no bytes skipped)
   n) page synced at current location; page length n bytes

*/

long ogg_sync_pageseek(ogg_sync_state *oy,ogg_page *og){
  unsigned char *page=oy->data+oy->returned;
  unsigned char *next;
  long bytes=oy->fill-oy->returned;

  if(ogg_sync_check(oy))return 0;

  if(oy->headerbytes==0){
    int headerbytes,i;
    if(bytes<27)return(0); /* not enough for a header */

    /* verify capture pattern */
    if(memcmp(page,"OggS",4))goto sync_fail;

    headerbytes=page[26]+27;
    if(bytes<headerbytes)return(0); /* not enough for header + seg table */

    /* count up body length in the segment table */

    for(i=0;i<page[26];i++)
      oy->bodybytes+=page[27+i];
    oy->headerbytes=headerbytes;
  }

  if(oy->bodybytes+oy->headerbytes>bytes)return(0);

  /* The whole test page is buffered.  Verify the checksum */
  {
    /* Grab the checksum bytes, set the header field to zero */
    char chksum[4];
    ogg_page log;

    memcpy(chksum,page+22,4);
    memset(page+22,0,4);

    /* set up a temp page struct and recompute the checksum */
    log.header=page;
    log.header_len=oy->headerbytes;
    log.body=page+oy->headerbytes;
    log.body_len=oy->bodybytes;
    ogg_page_checksum_set(&log);

    /* Compare */
    if(memcmp(chksum,page+22,4)){
      /* D'oh.  Mismatch! Corrupt page (or miscapture and not a page
         at all) */
      /* replace the computed checksum with the one actually read in */
      memcpy(page+22,chksum,4);

#ifndef DISABLE_CRC
      /* Bad checksum. Lose sync */
      goto sync_fail;
#endif
    }
  }

  /* yes, have a whole page all ready to go */
  {
    if(og){
      og->header=page;
      og->header_len=oy->headerbytes;
      og->body=page+oy->headerbytes;
      og->body_len=oy->bodybytes;
    }

    oy->unsynced=0;
    oy->returned+=(bytes=oy->headerbytes+oy->bodybytes);
    oy->headerbytes=0;
    oy->bodybytes=0;
    return(bytes);
  }

 sync_fail:

  oy->headerbytes=0;
  oy->bodybytes=0;

  /* search for possible capture */
  next=(unsigned char*)memchr(page+1,'O',bytes-1);
  if(!next)
    next=oy->data+oy->fill;

  oy->returned=(int)(next-oy->data);
  return((long)-(next-page));
}

/* sync the stream and get a page.  Keep trying until we find a page.
   Suppress 'sync errors' after reporting the first.

   return values:
   -1) recapture (hole in data)
    0) need more data
    1) page returned

   Returns pointers into buffered data; invalidated by next call to
   _stream, _clear, _init, or _buffer */

int ogg_sync_pageout(ogg_sync_state *oy, ogg_page *og){

  if(ogg_sync_check(oy))return 0;

  /* all we need to do is verify a page at the head of the stream
     buffer.  If it doesn't verify, we look for the next potential
     frame */

  for(;;){
    long ret=ogg_sync_pageseek(oy,og);
    if(ret>0){
      /* have a page */
      return(1);
    }
    if(ret==0){
      /* need more data */
      return(0);
    }

    /* head did not start a synced page... skipped some bytes */
    if(!oy->unsynced){
      oy->unsynced=1;
      return(-1);
    }

    /* loop. keep looking */

  }
}

/* add the incoming page to the stream state; we decompose the page
   into packet segments here as well. */

int ogg_stream_pagein(ogg_stream_state *os, ogg_page *og){
  unsigned char *header=og->header;
  unsigned char *body=og->body;
  long           bodysize=og->body_len;
  int            segptr=0;

  int version=ogg_page_version(og);
  int continued=ogg_page_continued(og);
  int bos=ogg_page_bos(og);
  int eos=ogg_page_eos(og);
  ogg_int64_t granulepos=ogg_page_granulepos(og);
  int serialno=ogg_page_serialno(og);
  long pageno=ogg_page_pageno(og);
  int segments=header[26];

  if(ogg_stream_check(os)) return -1;

  /* clean up 'returned data' */
  {
    long lr=os->lacing_returned;
    long br=os->body_returned;

    /* body data */
    if(br){
      os->body_fill-=br;
      if(os->body_fill)
        memmove(os->body_data,os->body_data+br,os->body_fill);
      os->body_returned=0;
    }

    if(lr){
      /* segment table */
      if(os->lacing_fill-lr){
        memmove(os->lacing_vals,os->lacing_vals+lr,
                (os->lacing_fill-lr)*sizeof(*os->lacing_vals));
        memmove(os->granule_vals,os->granule_vals+lr,
                (os->lacing_fill-lr)*sizeof(*os->granule_vals));
      }
      os->lacing_fill-=lr;
      os->lacing_packet-=lr;
      os->lacing_returned=0;
    }
  }

  /* check the serial number */
  if(serialno!=os->serialno)return(-1);
  if(version>0)return(-1);

  if(_os_lacing_expand(os,segments+1)) return -1;

  /* are we in sequence? */
  if(pageno!=os->pageno){
    int i;

    /* unroll previous partial packet (if any) */
    for(i=os->lacing_packet;i<os->lacing_fill;i++)
      os->body_fill-=os->lacing_vals[i]&0xff;
    os->lacing_fill=os->lacing_packet;

    /* make a note of dropped data in segment table */
    if(os->pageno!=-1){
      os->lacing_vals[os->lacing_fill++]=0x400;
      os->lacing_packet++;
    }
  }

  /* are we a 'continued packet' page?  If so, we may need to skip
     some segments */
  if(continued){
    if(os->lacing_fill<1 ||
       (os->lacing_vals[os->lacing_fill-1]&0xff)<255 ||
       os->lacing_vals[os->lacing_fill-1]==0x400){
      bos=0;
      for(;segptr<segments;segptr++){
        int val=header[27+segptr];
        body+=val;
        bodysize-=val;
        if(val<255){
          segptr++;
          break;
        }
      }
    }
  }

  if(bodysize){
    if(_os_body_expand(os,bodysize)) return -1;
    memcpy(os->body_data+os->body_fill,body,bodysize);
    os->body_fill+=bodysize;
  }

  {
    int saved=-1;
    while(segptr<segments){
      int val=header[27+segptr];
      os->lacing_vals[os->lacing_fill]=val;
      os->granule_vals[os->lacing_fill]=-1;

      if(bos){
        os->lacing_vals[os->lacing_fill]|=0x100;
        bos=0;
      }

      if(val<255)saved=os->lacing_fill;

      os->lacing_fill++;
      segptr++;

      if(val<255)os->lacing_packet=os->lacing_fill;
    }

    /* set the granulepos on the last granuleval of the last full packet */
    if(saved!=-1){
      os->granule_vals[saved]=granulepos;
    }

  }

  if(eos){
    os->e_o_s=1;
    if(os->lacing_fill>0)
      os->lacing_vals[os->lacing_fill-1]|=0x200;
  }

  os->pageno=pageno+1;

  return(0);
}

/* clear things to an initial state.  Good to call, eg, before seeking */
int ogg_sync_reset(ogg_sync_state *oy){
  if(ogg_sync_check(oy))return -1;

  oy->fill=0;
  oy->returned=0;
  oy->unsynced=0;
  oy->headerbytes=0;
  oy->bodybytes=0;
  return(0);
}

int ogg_stream_reset(ogg_stream_state *os){
  if(ogg_stream_check(os)) return -1;

  os->body_fill=0;
  os->body_returned=0;

  os->lacing_fill=0;
  os->lacing_packet=0;
  os->lacing_returned=0;

  os->header_fill=0;

  os->e_o_s=0;
  os->b_o_s=0;
  os->pageno=-1;
  os->packetno=0;
  os->granulepos=0;

  return(0);
}

int ogg_stream_reset_serialno(ogg_stream_state *os,int serialno){
  if(ogg_stream_check(os)) return -1;
  ogg_stream_reset(os);
  os->serialno=serialno;
  return(0);
}

static int _packetout(ogg_stream_state *os,ogg_packet *op,int adv){

  /* The last part of decode. We have the stream broken into packet
     segments.  Now we need to group them into packets (or return the
     out of sync markers) */

  int ptr=os->lacing_returned;

  if(os->lacing_packet<=ptr)return(0);

  if(os->lacing_vals[ptr]&0x400){
    /* we need to tell the codec there's a gap; it might need to
       handle previous packet dependencies. */
    os->lacing_returned++;
    os->packetno++;
    return(-1);
  }

  if(!op && !adv)return(1); /* just using peek as an inexpensive way
                               to ask if there's a whole packet
                               waiting */

  /* Gather the whole packet. We'll have no holes or a partial packet */
  {
    int size=os->lacing_vals[ptr]&0xff;
    long bytes=size;
    int eos=os->lacing_vals[ptr]&0x200; /* last packet of the stream? */
    int bos=os->lacing_vals[ptr]&0x100; /* first packet of the stream? */

    while(size==255){
      int val=os->lacing_vals[++ptr];
      size=val&0xff;
      if(val&0x200)eos=0x200;
      bytes+=size;
    }

    if(op){
      op->e_o_s=eos;
      op->b_o_s=bos;
      op->packet=os->body_data+os->body_returned;
      op->packetno=os->packetno;
      op->granulepos=os->granule_vals[ptr];
      op->bytes=bytes;
    }

    if(adv){
      os->body_returned+=bytes;
      os->lacing_returned=ptr+1;
      os->packetno++;
    }
  }
  return(1);
}

int ogg_stream_packetout(ogg_stream_state *os,ogg_packet *op){
  if(ogg_stream_check(os)) return 0;
  return _packetout(os,op,1);
}

int ogg_stream_packetpeek(ogg_stream_state *os,ogg_packet *op){
  if(ogg_stream_check(os)) return 0;
  return _packetout(os,op,0);
}

void ogg_packet_clear(ogg_packet *op) {
  _ogg_free(op->packet);
  memset(op, 0, sizeof(*op));
}

#ifdef _V_SELFTEST
#include <stdio.h>

ogg_stream_state os_en, os_de;
ogg_sync_state oy;

void checkpacket(ogg_packet *op,long len, int no, long pos){
  long j;
  static int sequence=0;
  static int lastno=0;

  if(op->bytes!=len){
    fprintf(stderr,"incorrect packet length (%ld != %ld)!\n",op->bytes,len);
    exit(1);
  }
  if(op->granulepos!=pos){
    fprintf(stderr,"incorrect packet granpos (%ld != %ld)!\n",(long)op->granulepos,pos);
    exit(1);
  }

  /* packet number just follows sequence/gap; adjust the input number
     for that */
  if(no==0){
    sequence=0;
  }else{
    sequence++;
    if(no>lastno+1)
      sequence++;
  }
  lastno=no;
  if(op->packetno!=sequence){
    fprintf(stderr,"incorrect packet sequence %ld != %d\n",
            (long)(op->packetno),sequence);
    exit(1);
  }

  /* Test data */
  for(j=0;j<op->bytes;j++)
    if(op->packet[j]!=((j+no)&0xff)){
      fprintf(stderr,"body data mismatch (1) at pos %ld: %x!=%lx!\n\n",
              j,op->packet[j],(j+no)&0xff);
      exit(1);
    }
}

void check_page(unsigned char *data,const int *header,ogg_page *og){
  long j;
  /* Test data */
  for(j=0;j<og->body_len;j++)
    if(og->body[j]!=data[j]){
      fprintf(stderr,"body data mismatch (2) at pos %ld: %x!=%x!\n\n",
              j,data[j],og->body[j]);
      exit(1);
    }

  /* Test header */
  for(j=0;j<og->header_len;j++){
    if(og->header[j]!=header[j]){
      fprintf(stderr,"header content mismatch at pos %ld:\n",j);
      for(j=0;j<header[26]+27;j++)
        fprintf(stderr," (%ld)%02x:%02x",j,header[j],og->header[j]);
      fprintf(stderr,"\n");
      exit(1);
    }
  }
  if(og->header_len!=header[26]+27){
    fprintf(stderr,"header length incorrect! (%ld!=%d)\n",
            og->header_len,header[26]+27);
    exit(1);
  }
}

void print_header(ogg_page *og){
  int j;
  fprintf(stderr,"\nHEADER:\n");
  fprintf(stderr,"  capture: %c %c %c %c  version: %d  flags: %x\n",
          og->header[0],og->header[1],og->header[2],og->header[3],
          (int)og->header[4],(int)og->header[5]);

  fprintf(stderr,"  granulepos: %d  serialno: %d  pageno: %ld\n",
          (og->header[9]<<24)|(og->header[8]<<16)|
          (og->header[7]<<8)|og->header[6],
          (og->header[17]<<24)|(og->header[16]<<16)|
          (og->header[15]<<8)|og->header[14],
          ((long)(og->header[21])<<24)|(og->header[20]<<16)|
          (og->header[19]<<8)|og->header[18]);

  fprintf(stderr,"  checksum: %02x:%02x:%02x:%02x\n  segments: %d (",
          (int)og->header[22],(int)og->header[23],
          (int)og->header[24],(int)og->header[25],
          (int)og->header[26]);

  for(j=27;j<og->header_len;j++)
    fprintf(stderr,"%d ",(int)og->header[j]);
  fprintf(stderr,")\n\n");
}

void copy_page(ogg_page *og){
  unsigned char *temp=_ogg_malloc(og->header_len);
  memcpy(temp,og->header,og->header_len);
  og->header=temp;

  temp=_ogg_malloc(og->body_len);
  memcpy(temp,og->body,og->body_len);
  og->body=temp;
}

void free_page(ogg_page *og){
  _ogg_free (og->header);
  _ogg_free (og->body);
}

void error(void){
  fprintf(stderr,"error!\n");
  exit(1);
}

/* 17 only */
const int head1_0[] = {0x4f,0x67,0x67,0x53,0,0x06,
                       0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,0,0,0,0,
                       0x15,0xed,0xec,0x91,
                       1,
                       17};

/* 17, 254, 255, 256, 500, 510, 600 byte, pad */
const int head1_1[] = {0x4f,0x67,0x67,0x53,0,0x02,
                       0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,0,0,0,0,
                       0x59,0x10,0x6c,0x2c,
                       1,
                       17};
const int head2_1[] = {0x4f,0x67,0x67,0x53,0,0x04,
                       0x07,0x18,0x00,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,1,0,0,0,
                       0x89,0x33,0x85,0xce,
                       13,
                       254,255,0,255,1,255,245,255,255,0,
                       255,255,90};

/* nil packets; beginning,middle,end */
const int head1_2[] = {0x4f,0x67,0x67,0x53,0,0x02,
                       0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,0,0,0,0,
                       0xff,0x7b,0x23,0x17,
                       1,
                       0};
const int head2_2[] = {0x4f,0x67,0x67,0x53,0,0x04,
                       0x07,0x28,0x00,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,1,0,0,0,
                       0x5c,0x3f,0x66,0xcb,
                       17,
                       17,254,255,0,0,255,1,0,255,245,255,255,0,
                       255,255,90,0};

/* large initial packet */
const int head1_3[] = {0x4f,0x67,0x67,0x53,0,0x02,
                       0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,0,0,0,0,
                       0x01,0x27,0x31,0xaa,
                       18,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,255,10};

const int head2_3[] = {0x4f,0x67,0x67,0x53,0,0x04,
                       0x07,0x08,0x00,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,1,0,0,0,
                       0x7f,0x4e,0x8a,0xd2,
                       4,
                       255,4,255,0};


/* continuing packet test */
const int head1_4[] = {0x4f,0x67,0x67,0x53,0,0x02,
                       0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,0,0,0,0,
                       0xff,0x7b,0x23,0x17,
                       1,
                       0};

const int head2_4[] = {0x4f,0x67,0x67,0x53,0,0x00,
                       0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
                       0x01,0x02,0x03,0x04,1,0,0,0,
                       0xf8,0x3c,0x19,0x79,
                       255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255};

const int head3_4[] = {0x4f,0x67,0x67,0x53,0,0x05,
                       0x07,0x0c,0x00,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,2,0,0,0,
                       0x38,0xe6,0xb6,0x28,
                       6,
                       255,220,255,4,255,0};


/* spill expansion test */
const int head1_4b[] = {0x4f,0x67,0x67,0x53,0,0x02,
                        0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                        0x01,0x02,0x03,0x04,0,0,0,0,
                        0xff,0x7b,0x23,0x17,
                        1,
                        0};

const int head2_4b[] = {0x4f,0x67,0x67,0x53,0,0x00,
                        0x07,0x10,0x00,0x00,0x00,0x00,0x00,0x00,
                        0x01,0x02,0x03,0x04,1,0,0,0,
                        0xce,0x8f,0x17,0x1a,
                        23,
                        255,255,255,255,255,255,255,255,
                        255,255,255,255,255,255,255,255,255,10,255,4,255,0,0};


const int head3_4b[] = {0x4f,0x67,0x67,0x53,0,0x04,
                        0x07,0x14,0x00,0x00,0x00,0x00,0x00,0x00,
                        0x01,0x02,0x03,0x04,2,0,0,0,
                        0x9b,0xb2,0x50,0xa1,
                        1,
                        0};

/* page with the 255 segment limit */
const int head1_5[] = {0x4f,0x67,0x67,0x53,0,0x02,
                       0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,0,0,0,0,
                       0xff,0x7b,0x23,0x17,
                       1,
                       0};

const int head2_5[] = {0x4f,0x67,0x67,0x53,0,0x00,
                       0x07,0xfc,0x03,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,1,0,0,0,
                       0xed,0x2a,0x2e,0xa7,
                       255,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10,10,
                       10,10,10,10,10,10,10};

const int head3_5[] = {0x4f,0x67,0x67,0x53,0,0x04,
                       0x07,0x00,0x04,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,2,0,0,0,
                       0x6c,0x3b,0x82,0x3d,
                       1,
                       50};


/* packet that overspans over an entire page */
const int head1_6[] = {0x4f,0x67,0x67,0x53,0,0x02,
                       0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,0,0,0,0,
                       0xff,0x7b,0x23,0x17,
                       1,
                       0};

const int head2_6[] = {0x4f,0x67,0x67,0x53,0,0x00,
                       0x07,0x04,0x00,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,1,0,0,0,
                       0x68,0x22,0x7c,0x3d,
                       255,
                       100,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255};

const int head3_6[] = {0x4f,0x67,0x67,0x53,0,0x01,
                       0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
                       0x01,0x02,0x03,0x04,2,0,0,0,
                       0xf4,0x87,0xba,0xf3,
                       255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255};

const int head4_6[] = {0x4f,0x67,0x67,0x53,0,0x05,
                       0x07,0x10,0x00,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,3,0,0,0,
                       0xf7,0x2f,0x6c,0x60,
                       5,
                       254,255,4,255,0};

/* packet that overspans over an entire page */
const int head1_7[] = {0x4f,0x67,0x67,0x53,0,0x02,
                       0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,0,0,0,0,
                       0xff,0x7b,0x23,0x17,
                       1,
                       0};

const int head2_7[] = {0x4f,0x67,0x67,0x53,0,0x00,
                       0x07,0x04,0x00,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,1,0,0,0,
                       0x68,0x22,0x7c,0x3d,
                       255,
                       100,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255,255,255,
                       255,255,255,255,255,255};

const int head3_7[] = {0x4f,0x67,0x67,0x53,0,0x05,
                       0x07,0x08,0x00,0x00,0x00,0x00,0x00,0x00,
                       0x01,0x02,0x03,0x04,2,0,0,0,
                       0xd4,0xe0,0x60,0xe5,
                       1,
                       0};

int compare_packet(const ogg_packet *op1, const ogg_packet *op2){
  if(op1->packet!=op2->packet){
    fprintf(stderr,"op1->packet != op2->packet\n");
    return(1);
  }
  if(op1->bytes!=op2->bytes){
    fprintf(stderr,"op1->bytes != op2->bytes\n");
    return(1);
  }
  if(op1->b_o_s!=op2->b_o_s){
    fprintf(stderr,"op1->b_o_s != op2->b_o_s\n");
    return(1);
  }
  if(op1->e_o_s!=op2->e_o_s){
    fprintf(stderr,"op1->e_o_s != op2->e_o_s\n");
    return(1);
  }
  if(op1->granulepos!=op2->granulepos){
    fprintf(stderr,"op1->granulepos != op2->granulepos\n");
    return(1);
  }
  if(op1->packetno!=op2->packetno){
    fprintf(stderr,"op1->packetno != op2->packetno\n");
    return(1);
  }
  return(0);
}

void test_pack(const int *pl, const int **headers, int byteskip,
               int pageskip, int packetskip){
  unsigned char *data=_ogg_malloc(1024*1024); /* for scripted test cases only */
  long inptr=0;
  long outptr=0;
  long deptr=0;
  long depacket=0;
  long granule_pos=7,pageno=0;
  int i,j,packets,pageout=pageskip;
  int eosflag=0;
  int bosflag=0;

  int byteskipcount=0;

  ogg_stream_reset(&os_en);
  ogg_stream_reset(&os_de);
  ogg_sync_reset(&oy);

  for(packets=0;packets<packetskip;packets++)
    depacket+=pl[packets];

  for(packets=0;;packets++)if(pl[packets]==-1)break;

  for(i=0;i<packets;i++){
    /* construct a test packet */
    ogg_packet op;
    int len=pl[i];

    op.packet=data+inptr;
    op.bytes=len;
    op.e_o_s=(pl[i+1]<0?1:0);
    op.granulepos=granule_pos;

    granule_pos+=1024;

    for(j=0;j<len;j++)data[inptr++]=i+j;

    /* submit the test packet */
    ogg_stream_packetin(&os_en,&op);

    /* retrieve any finished pages */
    {
      ogg_page og;

      while(ogg_stream_pageout(&os_en,&og)){
        /* We have a page.  Check it carefully */

        fprintf(stderr,"%ld, ",pageno);

        if(headers[pageno]==NULL){
          fprintf(stderr,"coded too many pages!\n");
          exit(1);
        }

        check_page(data+outptr,headers[pageno],&og);

        outptr+=og.body_len;
        pageno++;
        if(pageskip){
          bosflag=1;
          pageskip--;
          deptr+=og.body_len;
        }

        /* have a complete page; submit it to sync/decode */

        {
          ogg_page og_de;
          ogg_packet op_de,op_de2;
          char *buf=ogg_sync_buffer(&oy,og.header_len+og.body_len);
          char *next=buf;
          byteskipcount+=og.header_len;
          if(byteskipcount>byteskip){
            memcpy(next,og.header,byteskipcount-byteskip);
            next+=byteskipcount-byteskip;
            byteskipcount=byteskip;
          }

          byteskipcount+=og.body_len;
          if(byteskipcount>byteskip){
            memcpy(next,og.body,byteskipcount-byteskip);
            next+=byteskipcount-byteskip;
            byteskipcount=byteskip;
          }

          ogg_sync_wrote(&oy,next-buf);

          while(1){
            int ret=ogg_sync_pageout(&oy,&og_de);
            if(ret==0)break;
            if(ret<0)continue;
            /* got a page.  Happy happy.  Verify that it's good. */

            fprintf(stderr,"(%d), ",pageout);

            check_page(data+deptr,headers[pageout],&og_de);
            deptr+=og_de.body_len;
            pageout++;

            /* submit it to deconstitution */
            ogg_stream_pagein(&os_de,&og_de);

            /* packets out? */
            while(ogg_stream_packetpeek(&os_de,&op_de2)>0){
              ogg_stream_packetpeek(&os_de,NULL);
              ogg_stream_packetout(&os_de,&op_de); /* just catching them all */

              /* verify peek and out match */
              if(compare_packet(&op_de,&op_de2)){
                fprintf(stderr,"packetout != packetpeek! pos=%ld\n",
                        depacket);
                exit(1);
              }

              /* verify the packet! */
              /* check data */
              if(memcmp(data+depacket,op_de.packet,op_de.bytes)){
                fprintf(stderr,"packet data mismatch in decode! pos=%ld\n",
                        depacket);
                exit(1);
              }
              /* check bos flag */
              if(bosflag==0 && op_de.b_o_s==0){
                fprintf(stderr,"b_o_s flag not set on packet!\n");
                exit(1);
              }
              if(bosflag && op_de.b_o_s){
                fprintf(stderr,"b_o_s flag incorrectly set on packet!\n");
                exit(1);
              }
              bosflag=1;
              depacket+=op_de.bytes;

              /* check eos flag */
              if(eosflag){
                fprintf(stderr,"Multiple decoded packets with eos flag!\n");
                exit(1);
              }

              if(op_de.e_o_s)eosflag=1;

              /* check granulepos flag */
              if(op_de.granulepos!=-1){
                fprintf(stderr," granule:%ld ",(long)op_de.granulepos);
              }
            }
          }
        }
      }
    }
  }
  _ogg_free(data);
  if(headers[pageno]!=NULL){
    fprintf(stderr,"did not write last page!\n");
    exit(1);
  }
  if(headers[pageout]!=NULL){
    fprintf(stderr,"did not decode last page!\n");
    exit(1);
  }
  if(inptr!=outptr){
    fprintf(stderr,"encoded page data incomplete!\n");
    exit(1);
  }
  if(inptr!=deptr){
    fprintf(stderr,"decoded page data incomplete!\n");
    exit(1);
  }
  if(inptr!=depacket){
    fprintf(stderr,"decoded packet data incomplete!\n");
    exit(1);
  }
  if(!eosflag){
    fprintf(stderr,"Never got a packet with EOS set!\n");
    exit(1);
  }
  fprintf(stderr,"ok.\n");
}

int main(void){

  ogg_stream_init(&os_en,0x04030201);
  ogg_stream_init(&os_de,0x04030201);
  ogg_sync_init(&oy);

  /* Exercise each code path in the framing code.  Also verify that
     the checksums are working.  */

  {
    /* 17 only */
    const int packets[]={17, -1};
    const int *headret[]={head1_0,NULL};

    fprintf(stderr,"testing single page encoding... ");
    test_pack(packets,headret,0,0,0);
  }

  {
    /* 17, 254, 255, 256, 500, 510, 600 byte, pad */
    const int packets[]={17, 254, 255, 256, 500, 510, 600, -1};
    const int *headret[]={head1_1,head2_1,NULL};

    fprintf(stderr,"testing basic page encoding... ");
    test_pack(packets,headret,0,0,0);
  }

  {
    /* nil packets; beginning,middle,end */
    const int packets[]={0,17, 254, 255, 0, 256, 0, 500, 510, 600, 0, -1};
    const int *headret[]={head1_2,head2_2,NULL};

    fprintf(stderr,"testing basic nil packets... ");
    test_pack(packets,headret,0,0,0);
  }

  {
    /* large initial packet */
    const int packets[]={4345,259,255,-1};
    const int *headret[]={head1_3,head2_3,NULL};

    fprintf(stderr,"testing initial-packet lacing > 4k... ");
    test_pack(packets,headret,0,0,0);
  }

  {
    /* continuing packet test; with page spill expansion, we have to
       overflow the lacing table. */
    const int packets[]={0,65500,259,255,-1};
    const int *headret[]={head1_4,head2_4,head3_4,NULL};

    fprintf(stderr,"testing single packet page span... ");
    test_pack(packets,headret,0,0,0);
  }

  {
    /* spill expand packet test */
    const int packets[]={0,4345,259,255,0,0,-1};
    const int *headret[]={head1_4b,head2_4b,head3_4b,NULL};

    fprintf(stderr,"testing page spill expansion... ");
    test_pack(packets,headret,0,0,0);
  }

  /* page with the 255 segment limit */
  {

    const int packets[]={0,10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,10,
                   10,10,10,10,10,10,10,50,-1};
    const int *headret[]={head1_5,head2_5,head3_5,NULL};

    fprintf(stderr,"testing max packet segments... ");
    test_pack(packets,headret,0,0,0);
  }

  {
    /* packet that overspans over an entire page */
    const int packets[]={0,100,130049,259,255,-1};
    const int *headret[]={head1_6,head2_6,head3_6,head4_6,NULL};

    fprintf(stderr,"testing very large packets... ");
    test_pack(packets,headret,0,0,0);
  }

#ifndef DISABLE_CRC
  {
    /* test for the libogg 1.1.1 resync in large continuation bug
       found by Josh Coalson)  */
    const int packets[]={0,100,130049,259,255,-1};
    const int *headret[]={head1_6,head2_6,head3_6,head4_6,NULL};

    fprintf(stderr,"testing continuation resync in very large packets... ");
    test_pack(packets,headret,100,2,3);
  }
#else
    fprintf(stderr,"Skipping continuation resync test due to --disable-crc\n");
#endif

  {
    /* term only page.  why not? */
    const int packets[]={0,100,64770,-1};
    const int *headret[]={head1_7,head2_7,head3_7,NULL};

    fprintf(stderr,"testing zero data page (1 nil packet)... ");
    test_pack(packets,headret,0,0,0);
  }



  {
    /* build a bunch of pages for testing */
    unsigned char *data=_ogg_malloc(1024*1024);
    int pl[]={0, 1,1,98,4079, 1,1,2954,2057, 76,34,912,0,234,1000,1000, 1000,300,-1};
    int inptr=0,i,j;
    ogg_page og[5];

    ogg_stream_reset(&os_en);

    for(i=0;pl[i]!=-1;i++){
      ogg_packet op;
      int len=pl[i];

      op.packet=data+inptr;
      op.bytes=len;
      op.e_o_s=(pl[i+1]<0?1:0);
      op.granulepos=(i+1)*1000;

      for(j=0;j<len;j++)data[inptr++]=i+j;
      ogg_stream_packetin(&os_en,&op);
    }

    _ogg_free(data);

    /* retrieve finished pages */
    for(i=0;i<5;i++){
      if(ogg_stream_pageout(&os_en,&og[i])==0){
        fprintf(stderr,"Too few pages output building sync tests!\n");
        exit(1);
      }
      copy_page(&og[i]);
    }

    /* Test lost pages on pagein/packetout: no rollback */
    {
      ogg_page temp;
      ogg_packet test;

      fprintf(stderr,"Testing loss of pages... ");

      ogg_sync_reset(&oy);
      ogg_stream_reset(&os_de);
      for(i=0;i<5;i++){
        memcpy(ogg_sync_buffer(&oy,og[i].header_len),og[i].header,
               og[i].header_len);
        ogg_sync_wrote(&oy,og[i].header_len);
        memcpy(ogg_sync_buffer(&oy,og[i].body_len),og[i].body,og[i].body_len);
        ogg_sync_wrote(&oy,og[i].body_len);
      }

      ogg_sync_pageout(&oy,&temp);
      ogg_stream_pagein(&os_de,&temp);
      ogg_sync_pageout(&oy,&temp);
      ogg_stream_pagein(&os_de,&temp);
      ogg_sync_pageout(&oy,&temp);
      /* skip */
      ogg_sync_pageout(&oy,&temp);
      ogg_stream_pagein(&os_de,&temp);

      /* do we get the expected results/packets? */

      if(ogg_stream_packetout(&os_de,&test)!=1)error();
      checkpacket(&test,0,0,0);
      if(ogg_stream_packetout(&os_de,&test)!=1)error();
      checkpacket(&test,1,1,-1);
      if(ogg_stream_packetout(&os_de,&test)!=1)error();
      checkpacket(&test,1,2,-1);
      if(ogg_stream_packetout(&os_de,&test)!=1)error();
      checkpacket(&test,98,3,-1);
      if(ogg_stream_packetout(&os_de,&test)!=1)error();
      checkpacket(&test,4079,4,5000);
      if(ogg_stream_packetout(&os_de,&test)!=-1){
        fprintf(stderr,"Error: loss of page did not return error\n");
        exit(1);
      }
      if(ogg_stream_packetout(&os_de,&test)!=1)error();
      checkpacket(&test,76,9,-1);
      if(ogg_stream_packetout(&os_de,&test)!=1)error();
      checkpacket(&test,34,10,-1);
      fprintf(stderr,"ok.\n");
    }

    /* Test lost pages on pagein/packetout: rollback with continuation */
    {
      ogg_page temp;
      ogg_packet test;

      fprintf(stderr,"Testing loss of pages (rollback required)... ");

      ogg_sync_reset(&oy);
      ogg_stream_reset(&os_de);
      for(i=0;i<5;i++){
        memcpy(ogg_sync_buffer(&oy,og[i].header_len),og[i].header,
               og[i].header_len);
        ogg_sync_wrote(&oy,og[i].header_len);
        memcpy(ogg_sync_buffer(&oy,og[i].body_len),og[i].body,og[i].body_len);
        ogg_sync_wrote(&oy,og[i].body_len);
      }

      ogg_sync_pageout(&oy,&temp);
      ogg_stream_pagein(&os_de,&temp);
      ogg_sync_pageout(&oy,&temp);
      ogg_stream_pagein(&os_de,&temp);
      ogg_sync_pageout(&oy,&temp);
      ogg_stream_pagein(&os_de,&temp);
      ogg_sync_pageout(&oy,&temp);
      /* skip */
      ogg_sync_pageout(&oy,&temp);
      ogg_stream_pagein(&os_de,&temp);

      /* do we get the expected results/packets? */

      if(ogg_stream_packetout(&os_de,&test)!=1)error();
      checkpacket(&test,0,0,0);
      if(ogg_stream_packetout(&os_de,&test)!=1)error();
      checkpacket(&test,1,1,-1);
      if(ogg_stream_packetout(&os_de,&test)!=1)error();
      checkpacket(&test,1,2,-1);
      if(ogg_stream_packetout(&os_de,&test)!=1)error();
      checkpacket(&test,98,3,-1);
      if(ogg_stream_packetout(&os_de,&test)!=1)error();
      checkpacket(&test,4079,4,5000);
      if(ogg_stream_packetout(&os_de,&test)!=1)error();
      checkpacket(&test,1,5,-1);
      if(ogg_stream_packetout(&os_de,&test)!=1)error();
      checkpacket(&test,1,6,-1);
      if(ogg_stream_packetout(&os_de,&test)!=1)error();
      checkpacket(&test,2954,7,-1);
      if(ogg_stream_packetout(&os_de,&test)!=1)error();
      checkpacket(&test,2057,8,9000);
      if(ogg_stream_packetout(&os_de,&test)!=-1){
        fprintf(stderr,"Error: loss of page did not return error\n");
        exit(1);
      }
      if(ogg_stream_packetout(&os_de,&test)!=1)error();
      checkpacket(&test,300,17,18000);
      fprintf(stderr,"ok.\n");
    }

    /* the rest only test sync */
    {
      ogg_page og_de;
      /* Test fractional page inputs: incomplete capture */
      fprintf(stderr,"Testing sync on partial inputs... ");
      ogg_sync_reset(&oy);
      memcpy(ogg_sync_buffer(&oy,og[1].header_len),og[1].header,
             3);
      ogg_sync_wrote(&oy,3);
      if(ogg_sync_pageout(&oy,&og_de)>0)error();

      /* Test fractional page inputs: incomplete fixed header */
      memcpy(ogg_sync_buffer(&oy,og[1].header_len),og[1].header+3,
             20);
      ogg_sync_wrote(&oy,20);
      if(ogg_sync_pageout(&oy,&og_de)>0)error();

      /* Test fractional page inputs: incomplete header */
      memcpy(ogg_sync_buffer(&oy,og[1].header_len),og[1].header+23,
             5);
      ogg_sync_wrote(&oy,5);
      if(ogg_sync_pageout(&oy,&og_de)>0)error();

      /* Test fractional page inputs: incomplete body */

      memcpy(ogg_sync_buffer(&oy,og[1].header_len),og[1].header+28,
             og[1].header_len-28);
      ogg_sync_wrote(&oy,og[1].header_len-28);
      if(ogg_sync_pageout(&oy,&og_de)>0)error();

      memcpy(ogg_sync_buffer(&oy,og[1].body_len),og[1].body,1000);
      ogg_sync_wrote(&oy,1000);
      if(ogg_sync_pageout(&oy,&og_de)>0)error();

      memcpy(ogg_sync_buffer(&oy,og[1].body_len),og[1].body+1000,
             og[1].body_len-1000);
      ogg_sync_wrote(&oy,og[1].body_len-1000);
      if(ogg_sync_pageout(&oy,&og_de)<=0)error();

      fprintf(stderr,"ok.\n");
    }

    /* Test fractional page inputs: page + incomplete capture */
    {
      ogg_page og_de;
      fprintf(stderr,"Testing sync on 1+partial inputs... ");
      ogg_sync_reset(&oy);

      memcpy(ogg_sync_buffer(&oy,og[1].header_len),og[1].header,
             og[1].header_len);
      ogg_sync_wrote(&oy,og[1].header_len);

      memcpy(ogg_sync_buffer(&oy,og[1].body_len),og[1].body,
             og[1].body_len);
      ogg_sync_wrote(&oy,og[1].body_len);

      memcpy(ogg_sync_buffer(&oy,og[1].header_len),og[1].header,
             20);
      ogg_sync_wrote(&oy,20);
      if(ogg_sync_pageout(&oy,&og_de)<=0)error();
      if(ogg_sync_pageout(&oy,&og_de)>0)error();

      memcpy(ogg_sync_buffer(&oy,og[1].header_len),og[1].header+20,
             og[1].header_len-20);
      ogg_sync_wrote(&oy,og[1].header_len-20);
      memcpy(ogg_sync_buffer(&oy,og[1].body_len),og[1].body,
             og[1].body_len);
      ogg_sync_wrote(&oy,og[1].body_len);
      if(ogg_sync_pageout(&oy,&og_de)<=0)error();

      fprintf(stderr,"ok.\n");
    }

    /* Test recapture: garbage + page */
    {
      ogg_page og_de;
      fprintf(stderr,"Testing search for capture... ");
      ogg_sync_reset(&oy);

      /* 'garbage' */
      memcpy(ogg_sync_buffer(&oy,og[1].body_len),og[1].body,
             og[1].body_len);
      ogg_sync_wrote(&oy,og[1].body_len);

      memcpy(ogg_sync_buffer(&oy,og[1].header_len),og[1].header,
             og[1].header_len);
      ogg_sync_wrote(&oy,og[1].header_len);

      memcpy(ogg_sync_buffer(&oy,og[1].body_len),og[1].body,
             og[1].body_len);
      ogg_sync_wrote(&oy,og[1].body_len);

      memcpy(ogg_sync_buffer(&oy,og[2].header_len),og[2].header,
             20);
      ogg_sync_wrote(&oy,20);
      if(ogg_sync_pageout(&oy,&og_de)>0)error();
      if(ogg_sync_pageout(&oy,&og_de)<=0)error();
      if(ogg_sync_pageout(&oy,&og_de)>0)error();

      memcpy(ogg_sync_buffer(&oy,og[2].header_len),og[2].header+20,
             og[2].header_len-20);
      ogg_sync_wrote(&oy,og[2].header_len-20);
      memcpy(ogg_sync_buffer(&oy,og[2].body_len),og[2].body,
             og[2].body_len);
      ogg_sync_wrote(&oy,og[2].body_len);
      if(ogg_sync_pageout(&oy,&og_de)<=0)error();

      fprintf(stderr,"ok.\n");
    }

#ifndef DISABLE_CRC
    /* Test recapture: page + garbage + page */
    {
      ogg_page og_de;
      fprintf(stderr,"Testing recapture... ");
      ogg_sync_reset(&oy);

      memcpy(ogg_sync_buffer(&oy,og[1].header_len),og[1].header,
             og[1].header_len);
      ogg_sync_wrote(&oy,og[1].header_len);

      memcpy(ogg_sync_buffer(&oy,og[1].body_len),og[1].body,
             og[1].body_len);
      ogg_sync_wrote(&oy,og[1].body_len);

      memcpy(ogg_sync_buffer(&oy,og[2].header_len),og[2].header,
             og[2].header_len);
      ogg_sync_wrote(&oy,og[2].header_len);

      memcpy(ogg_sync_buffer(&oy,og[2].header_len),og[2].header,
             og[2].header_len);
      ogg_sync_wrote(&oy,og[2].header_len);

      if(ogg_sync_pageout(&oy,&og_de)<=0)error();

      memcpy(ogg_sync_buffer(&oy,og[2].body_len),og[2].body,
             og[2].body_len-5);
      ogg_sync_wrote(&oy,og[2].body_len-5);

      memcpy(ogg_sync_buffer(&oy,og[3].header_len),og[3].header,
             og[3].header_len);
      ogg_sync_wrote(&oy,og[3].header_len);

      memcpy(ogg_sync_buffer(&oy,og[3].body_len),og[3].body,
             og[3].body_len);
      ogg_sync_wrote(&oy,og[3].body_len);

      if(ogg_sync_pageout(&oy,&og_de)>0)error();
      if(ogg_sync_pageout(&oy,&og_de)<=0)error();

      fprintf(stderr,"ok.\n");
    }
#else
    fprintf(stderr,"Skipping recapture test due to --disable-crc\n");
#endif

    /* Free page data that was previously copied */
    {
      for(i=0;i<5;i++){
        free_page(&og[i]);
      }
    }
  }
  ogg_sync_clear(&oy);
  ogg_stream_clear(&os_en);
  ogg_stream_clear(&os_de);

  return(0);
}

#endif
#ifdef __cplusplus
}
#endif
#endif /* OGG_IMPL */
#ifdef VORBIS_IMPL
#ifdef __cplusplus
extern "C" {
#endif
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2015             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: miscellaneous prototypes

 ********************************************************************/

#ifndef _V_RANDOM_H_
#define _V_RANDOM_H_
/*#include "vorbis/codec.h"*/

extern void *_vorbis_block_alloc(vorbis_block *vb,long bytes);
extern void _vorbis_block_ripcord(vorbis_block *vb);
extern int ov_ilog(ogg_uint32_t v);

#ifdef ANALYSIS
extern int analysis_noisy;
extern void _analysis_output(char *base,int i,float *v,int n,int bark,int dB,
                             ogg_int64_t off);
extern void _analysis_output_always(char *base,int i,float *v,int n,int bark,int dB,
                             ogg_int64_t off);
#endif

#ifdef DEBUG_MALLOC

#define _VDBG_GRAPHFILE "malloc.m"
#undef _VDBG_GRAPHFILE
extern void *_VDBG_malloc(void *ptr,long bytes,char *file,long line);
extern void _VDBG_free(void *ptr,char *file,long line);

#ifndef MISC_C
#undef _ogg_malloc
#undef _ogg_calloc
#undef _ogg_realloc
#undef _ogg_free

#define _ogg_malloc(x) _VDBG_malloc(NULL,(x),__FILE__,__LINE__)
#define _ogg_calloc(x,y) _VDBG_malloc(NULL,(x)*(y),__FILE__,__LINE__)
#define _ogg_realloc(x,y) _VDBG_malloc((x),(y),__FILE__,__LINE__)
#define _ogg_free(x) _VDBG_free((x),__FILE__,__LINE__)
#endif
#endif

#endif




#ifndef _OS_H
#define _OS_H
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2015             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: #ifdef jail to whip a few platforms into the UNIX ideal.

 ********************************************************************/

#ifdef HAVE_CONFIG_H
/*#include "config.h"*/
#endif

#include <math.h>
/*#include <ogg/os_types.h>*/

/*#include "misc.h"*/

#ifndef _V_IFDEFJAIL_H_
#  define _V_IFDEFJAIL_H_

#  ifdef __GNUC__
#    define STIN static __inline__
#  elif defined(_WIN32)
#    define STIN static __inline
#  else
#    define STIN static
#  endif

#ifdef DJGPP
#  define rint(x)   (floor((x)+0.5f))
#endif

#ifndef M_PI
#  define M_PI (3.1415926536f)
#endif

#if defined(_WIN32) && !defined(__SYMBIAN32__)
#  include <malloc.h>
#  define rint(x)   (floor((x)+0.5f))
#  define NO_FLOAT_MATH_LIB
#  define FAST_HYPOT(a, b) sqrt((a)*(a) + (b)*(b))
#endif

#if defined(__SYMBIAN32__) && defined(__WINS__)
void *_alloca(size_t size);
#  define alloca _alloca
#endif

#ifndef FAST_HYPOT
#  define FAST_HYPOT hypot
#endif

#endif /* _V_IFDEFJAIL_H_ */

#ifdef HAVE_ALLOCA_H
#  include <alloca.h>
#endif

#ifdef USE_MEMORY_H
#  include <memory.h>
#endif

#ifndef min
#  define min(x,y)  ((x)>(y)?(y):(x))
#endif

#ifndef max
#  define max(x,y)  ((x)<(y)?(y):(x))
#endif


/* Special i386 GCC implementation */
#if defined(__i386__) && defined(__GNUC__) && !defined(__BEOS__) && !defined(__SSE2_MATH__)
#  define VORBIS_FPU_CONTROL
/* both GCC and MSVC are kinda stupid about rounding/casting to int.
   Because of encapsulation constraints (GCC can't see inside the asm
   block and so we end up doing stupid things like a store/load that
   is collectively a noop), we do it this way */

/* we must set up the fpu before this works!! */

typedef ogg_int16_t vorbis_fpu_control;

static inline void vorbis_fpu_setround(vorbis_fpu_control *fpu){
  ogg_int16_t ret;
  ogg_int16_t temp;
  __asm__ __volatile__("fnstcw %0\n\t"
          "movw %0,%%dx\n\t"
          "andw $62463,%%dx\n\t"
          "movw %%dx,%1\n\t"
          "fldcw %1\n\t":"=m"(ret):"m"(temp): "dx");
  *fpu=ret;
}

static inline void vorbis_fpu_restore(vorbis_fpu_control fpu){
  __asm__ __volatile__("fldcw %0":: "m"(fpu));
}

/* assumes the FPU is in round mode! */
static inline int vorbis_ftoi(double f){  /* yes, double!  Otherwise,
                                             we get extra fst/fld to
                                             truncate precision */
  int i;
  __asm__("fistl %0": "=m"(i) : "t"(f));
  return(i);
}
#endif /* Special i386 GCC implementation */


/* MSVC inline assembly. 32 bit only; inline ASM isn't implemented in the
 * 64 bit compiler and doesn't work on arm. */
#if defined(_MSC_VER) && defined(_M_IX86) && !defined(_WIN32_WCE)
#  define VORBIS_FPU_CONTROL

typedef ogg_int16_t vorbis_fpu_control;

static __inline int vorbis_ftoi(double f){
        int i;
        __asm{
                fld f
                fistp i
        }
        return i;
}

static __inline void vorbis_fpu_setround(vorbis_fpu_control *fpu){
  (void)fpu;
}

static __inline void vorbis_fpu_restore(vorbis_fpu_control fpu){
  (void)fpu;
}

#endif /* Special MSVC 32 bit implementation */


/* Optimized code path for x86_64 builds. Uses SSE2 intrinsics. This can be
   done safely because all x86_64 CPUs supports SSE2. */
#if (defined(_MSC_VER) && defined(_M_X64)) || (defined(__GNUC__) && defined (__SSE2_MATH__))
#  define VORBIS_FPU_CONTROL

typedef ogg_int16_t vorbis_fpu_control;

#include <emmintrin.h>
static __inline int vorbis_ftoi(double f){
        return _mm_cvtsd_si32(_mm_load_sd(&f));
}

static __inline void vorbis_fpu_setround(vorbis_fpu_control *fpu){
  (void)fpu;
}

static __inline void vorbis_fpu_restore(vorbis_fpu_control fpu){
  (void)fpu;
}

#endif /* Special MSVC x64 implementation */


/* If no special implementation was found for the current compiler / platform,
   use the default implementation here: */
#ifndef VORBIS_FPU_CONTROL

typedef int vorbis_fpu_control;

STIN int vorbis_ftoi(double f){
        /* Note: MSVC and GCC (at least on some systems) round towards zero, thus,
           the floor() call is required to ensure correct roudning of
           negative numbers */
        return (int)floor(f+.5);
}

/* We don't have special code for this compiler/arch, so do it the slow way */
#  define vorbis_fpu_setround(vorbis_fpu_control) {}
#  define vorbis_fpu_restore(vorbis_fpu_control) {}

#endif /* default implementation */

#endif /* _OS_H */
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: modified discrete cosine transform prototypes

 ********************************************************************/

#ifndef _OGG_mdct_H_
#define _OGG_mdct_H_

/*#include "vorbis/codec.h"*/





/*#define MDCT_INTEGERIZED  <- be warned there could be some hurt left here*/
#ifdef MDCT_INTEGERIZED

#define DATA_TYPE int
#define REG_TYPE  register int
#define TRIGBITS 14
#define cPI3_8 6270
#define cPI2_8 11585
#define cPI1_8 15137

#define FLOAT_CONV(x) ((int)((x)*(1<<TRIGBITS)+.5))
#define MULT_NORM(x) ((x)>>TRIGBITS)
#define HALVE(x) ((x)>>1)

#else

#define DATA_TYPE float
#define REG_TYPE  float
#define cPI3_8 .38268343236508977175F
#define cPI2_8 .70710678118654752441F
#define cPI1_8 .92387953251128675613F

#define FLOAT_CONV(x) (x)
#define MULT_NORM(x) (x)
#define HALVE(x) ((x)*.5f)

#endif


typedef struct {
  int n;
  int log2n;

  DATA_TYPE *trig;
  int       *bitrev;

  DATA_TYPE scale;
} mdct_lookup;

extern void mdct_init(mdct_lookup *lookup,int n);
extern void mdct_clear(mdct_lookup *l);
extern void mdct_forward(mdct_lookup *init, DATA_TYPE *in, DATA_TYPE *out);
extern void mdct_backward(mdct_lookup *init, DATA_TYPE *in, DATA_TYPE *out);

#endif
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: PCM data envelope analysis and manipulation

 ********************************************************************/

#ifndef _V_ENVELOPE_
#define _V_ENVELOPE_

/*#include "mdct.h"*/

#define VE_PRE    16
#define VE_WIN    4
#define VE_POST   2
#define VE_AMP    (VE_PRE+VE_POST-1)

#define VE_BANDS  7
#define VE_NEARDC 15

#define VE_MINSTRETCH 2   /* a bit less than short block */
#define VE_MAXSTRETCH 12  /* one-third full block */

typedef struct {
  float ampbuf[VE_AMP];
  int   ampptr;

  float nearDC[VE_NEARDC];
  float nearDC_acc;
  float nearDC_partialacc;
  int   nearptr;

} envelope_filter_state;

typedef struct {
  int begin;
  int end;
  float *window;
  float total;
} envelope_band;

typedef struct {
  int ch;
  int winlength;
  int searchstep;
  float minenergy;

  mdct_lookup  mdct;
  float       *mdct_win;

  envelope_band          band[VE_BANDS];
  envelope_filter_state *filter;
  int   stretch;

  int                   *mark;

  long storage;
  long current;
  long curmark;
  long cursor;
} envelope_lookup;

extern void _ve_envelope_init(envelope_lookup *e,vorbis_info *vi);
extern void _ve_envelope_clear(envelope_lookup *e);
extern long _ve_envelope_search(vorbis_dsp_state *v);
extern void _ve_envelope_shift(envelope_lookup *e,long shift);
extern int  _ve_envelope_mark(vorbis_dsp_state *v);


#endif
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2015             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: basic shared codebook operations

 ********************************************************************/

#ifndef _V_CODEBOOK_H_
#define _V_CODEBOOK_H_

/*#include <ogg/ogg.h>*/

/* This structure encapsulates huffman and VQ style encoding books; it
   doesn't do anything specific to either.

   valuelist/quantlist are nonNULL (and q_* significant) only if
   there's entry->value mapping to be done.

   If encode-side mapping must be done (and thus the entry needs to be
   hunted), the auxiliary encode pointer will point to a decision
   tree.  This is true of both VQ and huffman, but is mostly useful
   with VQ.

*/

typedef struct static_codebook{
  long   dim;           /* codebook dimensions (elements per vector) */
  long   entries;       /* codebook entries */
  char  *lengthlist;    /* codeword lengths in bits */

  /* mapping ***************************************************************/
  int    maptype;       /* 0=none
                           1=implicitly populated values from map column
                           2=listed arbitrary values */

  /* The below does a linear, single monotonic sequence mapping. */
  long     q_min;       /* packed 32 bit float; quant value 0 maps to minval */
  long     q_delta;     /* packed 32 bit float; val 1 - val 0 == delta */
  int      q_quant;     /* bits: 0 < quant <= 16 */
  int      q_sequencep; /* bitflag */

  long     *quantlist;  /* map == 1: (int)(entries^(1/dim)) element column map
                           map == 2: list of dim*entries quantized entry vals
                        */
  int allocedp;
} static_codebook;

typedef struct codebook{
  long dim;           /* codebook dimensions (elements per vector) */
  long entries;       /* codebook entries */
  long used_entries;  /* populated codebook entries */
  const static_codebook *c;

  /* for encode, the below are entry-ordered, fully populated */
  /* for decode, the below are ordered by _bitreversed codeword and only
     used entries are populated */
  float        *valuelist;  /* list of dim*entries actual entry values */
  ogg_uint32_t *codelist;   /* list of bitstream codewords for each entry */

  int          *dec_index;  /* only used if sparseness collapsed */
  char         *dec_codelengths;
  ogg_uint32_t *dec_firsttable;
  int           dec_firsttablen;
  int           dec_maxlength;

  /* The current encoder uses only centered, integer-only lattice books. */
  int           quantvals;
  int           minval;
  int           delta;
} codebook;

extern void vorbis_staticbook_destroy(static_codebook *b);
extern int vorbis_book_init_encode(codebook *dest,const static_codebook *source);
extern int vorbis_book_init_decode(codebook *dest,const static_codebook *source);
extern void vorbis_book_clear(codebook *b);

extern float *_book_unquantize(const static_codebook *b,int n,int *map);
extern float *_book_logdist(const static_codebook *b,float *vals);
extern float _float32_unpack(long val);
extern long   _float32_pack(float val);
extern int  _best(codebook *book, float *a, int step);
extern long _book_maptype1_quantvals(const static_codebook *b);

extern int vorbis_book_besterror(codebook *book,float *a,int step,int addmul);
extern long vorbis_book_codeword(codebook *book,int entry);
extern long vorbis_book_codelen(codebook *book,int entry);



extern int vorbis_staticbook_pack(const static_codebook *c,oggpack_buffer *b);
extern static_codebook *vorbis_staticbook_unpack(oggpack_buffer *b);

extern int vorbis_book_encode(codebook *book, int a, oggpack_buffer *b);

extern long vorbis_book_decode(codebook *book, oggpack_buffer *b);
extern long vorbis_book_decodevs_add(codebook *book, float *a,
                                     oggpack_buffer *b,int n);
extern long vorbis_book_decodev_set(codebook *book, float *a,
                                    oggpack_buffer *b,int n);
extern long vorbis_book_decodev_add(codebook *book, float *a,
                                    oggpack_buffer *b,int n);
extern long vorbis_book_decodevv_add(codebook *book, float **a,
                                     long off,int ch,
                                    oggpack_buffer *b,int n);



#endif
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2007             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: fft transform

 ********************************************************************/

#ifndef _V_SMFT_H_
#define _V_SMFT_H_

/*#include "vorbis/codec.h"*/

typedef struct {
  int n;
  float *trigcache;
  int *splitcache;
} drft_lookup;

extern void drft_forward(drft_lookup *l,float *data);
extern void drft_backward(drft_lookup *l,float *data);
extern void drft_init(drft_lookup *l,int n);
extern void drft_clear(drft_lookup *l);

#endif
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: libvorbis codec headers

 ********************************************************************/

#ifndef _V_CODECI_H_
#define _V_CODECI_H_

/*#include "envelope.h"*/
/*#include "codebook.h"*/

#define BLOCKTYPE_IMPULSE    0
#define BLOCKTYPE_PADDING    1
#define BLOCKTYPE_TRANSITION 0
#define BLOCKTYPE_LONG       1

#define PACKETBLOBS 15

typedef struct vorbis_block_internal{
  float  **pcmdelay;  /* this is a pointer into local storage */
  float  ampmax;
  int    blocktype;

  oggpack_buffer *packetblob[PACKETBLOBS]; /* initialized, must be freed;
                                              blob [PACKETBLOBS/2] points to
                                              the oggpack_buffer in the
                                              main vorbis_block */
} vorbis_block_internal;

typedef void vorbis_look_floor;
typedef void vorbis_look_residue;
typedef void vorbis_look_transform;

/* mode ************************************************************/
typedef struct {
  int blockflag;
  int windowtype;
  int transformtype;
  int mapping;
} vorbis_info_mode;

typedef void vorbis_info_floor;
typedef void vorbis_info_residue;
typedef void vorbis_info_mapping;

/*#include "psy.h"*/
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: random psychoacoustics (not including preecho)

 ********************************************************************/

#ifndef _V_PSY_H_
#define _V_PSY_H_
/*#include "smallft.h"*/

/*#include "backends.h"*/
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: libvorbis backend and mapping structures; needed for
           static mode headers

 ********************************************************************/

/* this is exposed up here because we need it for static modes.
   Lookups for each backend aren't exposed because there's no reason
   to do so */

#ifndef _vorbis_backend_h_
#define _vorbis_backend_h_

/*#include "codec_internal.h"*/

/* this would all be simpler/shorter with templates, but.... */
/* Floor backend generic *****************************************/
typedef struct{
  void                   (*pack)  (vorbis_info_floor *,oggpack_buffer *);
  vorbis_info_floor     *(*unpack)(vorbis_info *,oggpack_buffer *);
  vorbis_look_floor     *(*look)  (vorbis_dsp_state *,vorbis_info_floor *);
  void (*free_info) (vorbis_info_floor *);
  void (*free_look) (vorbis_look_floor *);
  void *(*inverse1)  (struct vorbis_block *,vorbis_look_floor *);
  int   (*inverse2)  (struct vorbis_block *,vorbis_look_floor *,
                     void *buffer,float *);
} vorbis_func_floor;

typedef struct{
  int   order;
  long  rate;
  long  barkmap;

  int   ampbits;
  int   ampdB;

  int   numbooks; /* <= 16 */
  int   books[16];

  float lessthan;     /* encode-only config setting hacks for libvorbis */
  float greaterthan;  /* encode-only config setting hacks for libvorbis */

} vorbis_info_floor0;


#define VIF_POSIT 63
#define VIF_CLASS 16
#define VIF_PARTS 31
typedef struct{
  int   partitions;                /* 0 to 31 */
  int   partitionclass[VIF_PARTS]; /* 0 to 15 */

  int   class_dim[VIF_CLASS];        /* 1 to 8 */
  int   class_subs[VIF_CLASS];       /* 0,1,2,3 (bits: 1<<n poss) */
  int   class_book[VIF_CLASS];       /* subs ^ dim entries */
  int   class_subbook[VIF_CLASS][8]; /* [VIF_CLASS][subs] */


  int   mult;                      /* 1 2 3 or 4 */
  int   postlist[VIF_POSIT+2];    /* first two implicit */


  /* encode side analysis parameters */
  float maxover;
  float maxunder;
  float maxerr;

  float twofitweight;
  float twofitatten;

  int   n;

} vorbis_info_floor1;

/* Residue backend generic *****************************************/
typedef struct{
  void                 (*pack)  (vorbis_info_residue *,oggpack_buffer *);
  vorbis_info_residue *(*unpack)(vorbis_info *,oggpack_buffer *);
  vorbis_look_residue *(*look)  (vorbis_dsp_state *,
                                 vorbis_info_residue *);
  void (*free_info)    (vorbis_info_residue *);
  void (*free_look)    (vorbis_look_residue *);
  long **(*class_)      (struct vorbis_block *,vorbis_look_residue *,
                        int **,int *,int);
  int  (*forward)      (oggpack_buffer *,struct vorbis_block *,
                        vorbis_look_residue *,
                        int **,int *,int,long **,int);
  int  (*inverse)      (struct vorbis_block *,vorbis_look_residue *,
                        float **,int *,int);
} vorbis_func_residue;

typedef struct vorbis_info_residue0{
/* block-partitioned VQ coded straight residue */
  long  begin;
  long  end;

  /* first stage (lossless partitioning) */
  int    grouping;         /* group n vectors per partition */
  int    partitions;       /* possible codebooks for a partition */
  int    partvals;         /* partitions ^ groupbook dim */
  int    groupbook;        /* huffbook for partitioning */
  int    secondstages[64]; /* expanded out to pointers in lookup */
  int    booklist[512];    /* list of second stage books */

  const int classmetric1[64];
  const int classmetric2[64];
} vorbis_info_residue0;

/* Mapping backend generic *****************************************/
typedef struct{
  void                 (*pack)  (vorbis_info *,vorbis_info_mapping *,
                                 oggpack_buffer *);
  vorbis_info_mapping *(*unpack)(vorbis_info *,oggpack_buffer *);
  void (*free_info)    (vorbis_info_mapping *);
  int  (*forward)      (struct vorbis_block *vb);
  int  (*inverse)      (struct vorbis_block *vb,vorbis_info_mapping *);
} vorbis_func_mapping;

typedef struct vorbis_info_mapping0{
  int   submaps;  /* <= 16 */
  int   chmuxlist[256];   /* up to 256 channels in a Vorbis stream */

  int   floorsubmap[16];   /* [mux] submap to floors */
  int   residuesubmap[16]; /* [mux] submap to residue */

  int   coupling_steps;
  int   coupling_mag[256];
  int   coupling_ang[256];

} vorbis_info_mapping0;

#endif
/*#include "envelope.h"*/

#ifndef EHMER_MAX
#define EHMER_MAX 56
#endif

/* psychoacoustic setup ********************************************/
#define P_BANDS 17      /* 62Hz to 16kHz */
#define P_LEVELS 8      /* 30dB to 100dB */
#define P_LEVEL_0 30.    /* 30 dB */
#define P_NOISECURVES 3

#define NOISE_COMPAND_LEVELS 40
typedef struct vorbis_info_psy{
  int   blockflag;

  float ath_adjatt;
  float ath_maxatt;

  float tone_masteratt[P_NOISECURVES];
  float tone_centerboost;
  float tone_decay;
  float tone_abs_limit;
  float toneatt[P_BANDS];

  int noisemaskp;
  float noisemaxsupp;
  float noisewindowlo;
  float noisewindowhi;
  int   noisewindowlomin;
  int   noisewindowhimin;
  int   noisewindowfixed;
  float noiseoff[P_NOISECURVES][P_BANDS];
  float noisecompand[NOISE_COMPAND_LEVELS];

  float max_curve_dB;

  int normal_p;
  int normal_start;
  int normal_partition;
  double normal_thresh;
} vorbis_info_psy;

typedef struct{
  int   eighth_octave_lines;

  /* for block long/short tuning; encode only */
  float preecho_thresh[VE_BANDS];
  float postecho_thresh[VE_BANDS];
  float stretch_penalty;
  float preecho_minenergy;

  float ampmax_att_per_sec;

  /* channel coupling config */
  int   coupling_pkHz[PACKETBLOBS];
  int   coupling_pointlimit[2][PACKETBLOBS];
  int   coupling_prepointamp[PACKETBLOBS];
  int   coupling_postpointamp[PACKETBLOBS];
  int   sliding_lowpass[2][PACKETBLOBS];

} vorbis_info_psy_global;

typedef struct {
  float ampmax;
  int   channels;

  vorbis_info_psy_global *gi;
  int   coupling_pointlimit[2][P_NOISECURVES];
} vorbis_look_psy_global;


typedef struct {
  int n;
  struct vorbis_info_psy *vi;

  float ***tonecurves;
  float **noiseoffset;

  float *ath;
  long  *octave;             /* in n.ocshift format */
  long  *bark;

  long  firstoc;
  long  shiftoc;
  int   eighth_octave_lines; /* power of two, please */
  int   total_octave_lines;
  long  rate; /* cache it */

  float m_val; /* Masking compensation value */

} vorbis_look_psy;

extern void   _vp_psy_init(vorbis_look_psy *p,vorbis_info_psy *vi,
                           vorbis_info_psy_global *gi,int n,long rate);
extern void   _vp_psy_clear(vorbis_look_psy *p);
extern void  *_vi_psy_dup(void *source);

extern void   _vi_psy_free(vorbis_info_psy *i);
extern vorbis_info_psy *_vi_psy_copy(vorbis_info_psy *i);

extern void _vp_noisemask(vorbis_look_psy *p,
                          float *logmdct,
                          float *logmask);

extern void _vp_tonemask(vorbis_look_psy *p,
                         float *logfft,
                         float *logmask,
                         float global_specmax,
                         float local_specmax);

extern void _vp_offset_and_mix(vorbis_look_psy *p,
                               float *noise,
                               float *tone,
                               int offset_select,
                               float *logmask,
                               float *mdct,
                               float *logmdct);

extern float _vp_ampmax_decay(float amp,vorbis_dsp_state *vd);

extern void _vp_couple_quantize_normalize(int blobno,
                                          vorbis_info_psy_global *g,
                                          vorbis_look_psy *p,
                                          vorbis_info_mapping0 *vi,
                                          float **mdct,
                                          int   **iwork,
                                          int    *nonzero,
                                          int     sliding_lowpass,
                                          int     ch);

#endif
/*#include "bitrate.h"*/
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2007             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: bitrate tracking and management

 ********************************************************************/

#ifndef _V_BITRATE_H_
#define _V_BITRATE_H_

/*#include "vorbis/codec.h"*/
/*#include "codec_internal.h"*/
/*#include "os.h"*/

/* encode side bitrate tracking */
typedef struct bitrate_manager_state {
  int            managed;

  long           avg_reservoir;
  long           minmax_reservoir;
  long           avg_bitsper;
  long           min_bitsper;
  long           max_bitsper;

  long           short_per_long;
  double         avgfloat;

  vorbis_block  *vb;
  int            choice;
} bitrate_manager_state;

typedef struct bitrate_manager_info{
  long           avg_rate;
  long           min_rate;
  long           max_rate;
  long           reservoir_bits;
  double         reservoir_bias;

  double         slew_damp;

} bitrate_manager_info;

extern void vorbis_bitrate_init(vorbis_info *vi,bitrate_manager_state *bs);
extern void vorbis_bitrate_clear(bitrate_manager_state *bs);
extern int vorbis_bitrate_managed(vorbis_block *vb);
extern int vorbis_bitrate_addblock(vorbis_block *vb);
extern int vorbis_bitrate_flushpacket(vorbis_dsp_state *vd, ogg_packet *op);

#endif

typedef struct private_state {
  /* local lookup storage */
  envelope_lookup        *ve; /* envelope lookup */
  int                     window[2];
  vorbis_look_transform **transform[2];    /* block, type */
  drft_lookup             fft_look[2];

  int                     modebits;
  vorbis_look_floor     **flr;
  vorbis_look_residue   **residue;
  vorbis_look_psy        *psy;
  vorbis_look_psy_global *psy_g_look;

  /* local storage, only used on the encoding side.  This way the
     application does not need to worry about freeing some packets'
     memory and not others'; packet storage is always tracked.
     Cleared next call to a _dsp_ function */
  unsigned char *header;
  unsigned char *header1;
  unsigned char *header2;

  bitrate_manager_state bms;

  ogg_int64_t sample_count;
} private_state;

/* codec_setup_info contains all the setup information specific to the
   specific compression/decompression mode in progress (eg,
   psychoacoustic settings, channel setup, options, codebook
   etc).
*********************************************************************/

/*#include "highlevel.h"*/
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: highlevel encoder setup struct separated out for vorbisenc clarity

 ********************************************************************/

typedef struct highlevel_byblocktype {
  double tone_mask_setting;
  double tone_peaklimit_setting;
  double noise_bias_setting;
  double noise_compand_setting;
} highlevel_byblocktype;

typedef struct highlevel_encode_setup {
  int   set_in_stone;
  const void *setup;
  double base_setting;

  double impulse_noisetune;

  /* bitrate management below all settable */
  float  req;
  int    managed;
  long   bitrate_min;
  long   bitrate_av;
  double bitrate_av_damp;
  long   bitrate_max;
  long   bitrate_reservoir;
  double bitrate_reservoir_bias;

  int impulse_block_p;
  int noise_normalize_p;
  int coupling_p;

  double stereo_point_setting;
  double lowpass_kHz;
  int    lowpass_altered;

  double ath_floating_dB;
  double ath_absolute_dB;

  double amplitude_track_dBpersec;
  double trigger_setting;

  highlevel_byblocktype block[4]; /* padding, impulse, transition, long */

} highlevel_encode_setup;
typedef struct codec_setup_info {

  /* Vorbis supports only short and long blocks, but allows the
     encoder to choose the sizes */

  long blocksizes[2];

  /* modes are the primary means of supporting on-the-fly different
     blocksizes, different channel mappings (LR or M/A),
     different residue backends, etc.  Each mode consists of a
     blocksize flag and a mapping (along with the mapping setup */

  int        modes;
  int        maps;
  int        floors;
  int        residues;
  int        books;
  int        psys;     /* encode only */

  vorbis_info_mode       *mode_param[64];
  int                     map_type[64];
  vorbis_info_mapping    *map_param[64];
  int                     floor_type[64];
  vorbis_info_floor      *floor_param[64];
  int                     residue_type[64];
  vorbis_info_residue    *residue_param[64];
  static_codebook        *book_param[256];
  codebook               *fullbooks;

  vorbis_info_psy        *psy_param[4]; /* encode only */
  vorbis_info_psy_global psy_g_param;

  bitrate_manager_info   bi;
  highlevel_encode_setup hi; /* used only by vorbisenc.c.  It's a
                                highly redundant structure, but
                                improves clarity of program flow. */
  int         halfrate_flag; /* painless downsample for decode */
} codec_setup_info;

extern vorbis_look_psy_global *_vp_global_look(vorbis_info *vi);
extern void _vp_global_free(vorbis_look_psy_global *look);



typedef struct {
  int sorted_index[VIF_POSIT+2];
  int forward_index[VIF_POSIT+2];
  int reverse_index[VIF_POSIT+2];

  int hineighbor[VIF_POSIT];
  int loneighbor[VIF_POSIT];
  int posts;

  int n;
  int quant_q;
  vorbis_info_floor1 *vi;

  long phrasebits;
  long postbits;
  long frames;
} vorbis_look_floor1;



extern int *floor1_fit(vorbis_block *vb,vorbis_look_floor1 *look,
                          const float *logmdct,   /* in */
                          const float *logmask);
extern int *floor1_interpolate_fit(vorbis_block *vb,vorbis_look_floor1 *look,
                          int *A,int *B,
                          int del);
extern int floor1_encode(oggpack_buffer *opb,vorbis_block *vb,
                  vorbis_look_floor1 *look,
                  int *post,int *ilogmask);
#endif
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2007             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

  function: lookup data; generated by lookups.pl; edit there

 ********************************************************************/

#ifndef _V_LOOKUP_DATA_H_

#ifdef FLOAT_LOOKUP
#define COS_LOOKUP_SZ 128
static const float COS_LOOKUP[COS_LOOKUP_SZ+1]={
        +1.0000000000000f,+0.9996988186962f,+0.9987954562052f,+0.9972904566787f,
        +0.9951847266722f,+0.9924795345987f,+0.9891765099648f,+0.9852776423889f,
        +0.9807852804032f,+0.9757021300385f,+0.9700312531945f,+0.9637760657954f,
        +0.9569403357322f,+0.9495281805930f,+0.9415440651830f,+0.9329927988347f,
        +0.9238795325113f,+0.9142097557035f,+0.9039892931234f,+0.8932243011955f,
        +0.8819212643484f,+0.8700869911087f,+0.8577286100003f,+0.8448535652497f,
        +0.8314696123025f,+0.8175848131516f,+0.8032075314806f,+0.7883464276266f,
        +0.7730104533627f,+0.7572088465065f,+0.7409511253550f,+0.7242470829515f,
        +0.7071067811865f,+0.6895405447371f,+0.6715589548470f,+0.6531728429538f,
        +0.6343932841636f,+0.6152315905806f,+0.5956993044924f,+0.5758081914178f,
        +0.5555702330196f,+0.5349976198871f,+0.5141027441932f,+0.4928981922298f,
        +0.4713967368260f,+0.4496113296546f,+0.4275550934303f,+0.4052413140050f,
        +0.3826834323651f,+0.3598950365350f,+0.3368898533922f,+0.3136817403989f,
        +0.2902846772545f,+0.2667127574749f,+0.2429801799033f,+0.2191012401569f,
        +0.1950903220161f,+0.1709618887603f,+0.1467304744554f,+0.1224106751992f,
        +0.0980171403296f,+0.0735645635997f,+0.0490676743274f,+0.0245412285229f,
        +0.0000000000000f,-0.0245412285229f,-0.0490676743274f,-0.0735645635997f,
        -0.0980171403296f,-0.1224106751992f,-0.1467304744554f,-0.1709618887603f,
        -0.1950903220161f,-0.2191012401569f,-0.2429801799033f,-0.2667127574749f,
        -0.2902846772545f,-0.3136817403989f,-0.3368898533922f,-0.3598950365350f,
        -0.3826834323651f,-0.4052413140050f,-0.4275550934303f,-0.4496113296546f,
        -0.4713967368260f,-0.4928981922298f,-0.5141027441932f,-0.5349976198871f,
        -0.5555702330196f,-0.5758081914178f,-0.5956993044924f,-0.6152315905806f,
        -0.6343932841636f,-0.6531728429538f,-0.6715589548470f,-0.6895405447371f,
        -0.7071067811865f,-0.7242470829515f,-0.7409511253550f,-0.7572088465065f,
        -0.7730104533627f,-0.7883464276266f,-0.8032075314806f,-0.8175848131516f,
        -0.8314696123025f,-0.8448535652497f,-0.8577286100003f,-0.8700869911087f,
        -0.8819212643484f,-0.8932243011955f,-0.9039892931234f,-0.9142097557035f,
        -0.9238795325113f,-0.9329927988347f,-0.9415440651830f,-0.9495281805930f,
        -0.9569403357322f,-0.9637760657954f,-0.9700312531945f,-0.9757021300385f,
        -0.9807852804032f,-0.9852776423889f,-0.9891765099648f,-0.9924795345987f,
        -0.9951847266722f,-0.9972904566787f,-0.9987954562052f,-0.9996988186962f,
        -1.0000000000000f,
};

#define INVSQ_LOOKUP_SZ 32
static const float INVSQ_LOOKUP[INVSQ_LOOKUP_SZ+1]={
        1.414213562373f,1.392621247646f,1.371988681140f,1.352246807566f,
        1.333333333333f,1.315191898443f,1.297771369046f,1.281025230441f,
        1.264911064067f,1.249390095109f,1.234426799697f,1.219988562661f,
        1.206045378311f,1.192569588000f,1.179535649239f,1.166919931983f,
        1.154700538379f,1.142857142857f,1.131370849898f,1.120224067222f,
        1.109400392450f,1.098884511590f,1.088662107904f,1.078719779941f,
        1.069044967650f,1.059625885652f,1.050451462878f,1.041511287847f,
        1.032795558989f,1.024295039463f,1.016001016002f,1.007905261358f,
        1.000000000000f,
};

#define INVSQ2EXP_LOOKUP_MIN (-32)
#define INVSQ2EXP_LOOKUP_MAX 32
static const float INVSQ2EXP_LOOKUP[INVSQ2EXP_LOOKUP_MAX-\
                              INVSQ2EXP_LOOKUP_MIN+1]={
                 65536.f,    46340.95001f,         32768.f,    23170.47501f,
                 16384.f,     11585.2375f,          8192.f,    5792.618751f,
                  4096.f,    2896.309376f,          2048.f,    1448.154688f,
                  1024.f,    724.0773439f,           512.f,     362.038672f,
                   256.f,     181.019336f,           128.f,    90.50966799f,
                    64.f,      45.254834f,            32.f,      22.627417f,
                    16.f,     11.3137085f,             8.f,    5.656854249f,
                     4.f,    2.828427125f,             2.f,    1.414213562f,
                     1.f,   0.7071067812f,            0.5f,   0.3535533906f,
                   0.25f,   0.1767766953f,          0.125f,  0.08838834765f,
                 0.0625f,  0.04419417382f,        0.03125f,  0.02209708691f,
               0.015625f,  0.01104854346f,      0.0078125f, 0.005524271728f,
             0.00390625f, 0.002762135864f,    0.001953125f, 0.001381067932f,
           0.0009765625f, 0.000690533966f,  0.00048828125f, 0.000345266983f,
         0.000244140625f,0.0001726334915f,0.0001220703125f,8.631674575e-05f,
        6.103515625e-05f,4.315837288e-05f,3.051757812e-05f,2.157918644e-05f,
        1.525878906e-05f,
};

#endif

#define FROMdB_LOOKUP_SZ 35
#define FROMdB2_LOOKUP_SZ 32
#define FROMdB_SHIFT 5
#define FROMdB2_SHIFT 3
#define FROMdB2_MASK 31

#ifdef FLOAT_LOOKUP
static const float FROMdB_LOOKUP[FROMdB_LOOKUP_SZ]={
                     1.f,   0.6309573445f,   0.3981071706f,   0.2511886432f,
           0.1584893192f,            0.1f,  0.06309573445f,  0.03981071706f,
          0.02511886432f,  0.01584893192f,           0.01f, 0.006309573445f,
         0.003981071706f, 0.002511886432f, 0.001584893192f,          0.001f,
        0.0006309573445f,0.0003981071706f,0.0002511886432f,0.0001584893192f,
                 0.0001f,6.309573445e-05f,3.981071706e-05f,2.511886432e-05f,
        1.584893192e-05f,          1e-05f,6.309573445e-06f,3.981071706e-06f,
        2.511886432e-06f,1.584893192e-06f,          1e-06f,6.309573445e-07f,
        3.981071706e-07f,2.511886432e-07f,1.584893192e-07f,
};

static const float FROMdB2_LOOKUP[FROMdB2_LOOKUP_SZ]={
           0.9928302478f,   0.9786445908f,   0.9646616199f,   0.9508784391f,
           0.9372921937f,     0.92390007f,   0.9106992942f,   0.8976871324f,
           0.8848608897f,   0.8722179097f,   0.8597555737f,   0.8474713009f,
            0.835362547f,   0.8234268041f,   0.8116616003f,   0.8000644989f,
           0.7886330981f,   0.7773650302f,   0.7662579617f,    0.755309592f,
           0.7445176537f,   0.7338799116f,   0.7233941627f,   0.7130582353f,
           0.7028699885f,   0.6928273125f,   0.6829281272f,   0.6731703824f,
           0.6635520573f,   0.6540711597f,   0.6447257262f,   0.6355138211f,
};
#endif

#ifdef INT_LOOKUP

#define INVSQ_LOOKUP_I_SHIFT 10
#define INVSQ_LOOKUP_I_MASK 1023
static const long INVSQ_LOOKUP_I[64+1]={
           92682l,   91966l,   91267l,   90583l,
           89915l,   89261l,   88621l,   87995l,
           87381l,   86781l,   86192l,   85616l,
           85051l,   84497l,   83953l,   83420l,
           82897l,   82384l,   81880l,   81385l,
           80899l,   80422l,   79953l,   79492l,
           79039l,   78594l,   78156l,   77726l,
           77302l,   76885l,   76475l,   76072l,
           75674l,   75283l,   74898l,   74519l,
           74146l,   73778l,   73415l,   73058l,
           72706l,   72359l,   72016l,   71679l,
           71347l,   71019l,   70695l,   70376l,
           70061l,   69750l,   69444l,   69141l,
           68842l,   68548l,   68256l,   67969l,
           67685l,   67405l,   67128l,   66855l,
           66585l,   66318l,   66054l,   65794l,
           65536l,
};

#define COS_LOOKUP_I_SHIFT 9
#define COS_LOOKUP_I_MASK 511
#define COS_LOOKUP_I_SZ 128
static const long COS_LOOKUP_I[COS_LOOKUP_I_SZ+1]={
           16384l,   16379l,   16364l,   16340l,
           16305l,   16261l,   16207l,   16143l,
           16069l,   15986l,   15893l,   15791l,
           15679l,   15557l,   15426l,   15286l,
           15137l,   14978l,   14811l,   14635l,
           14449l,   14256l,   14053l,   13842l,
           13623l,   13395l,   13160l,   12916l,
           12665l,   12406l,   12140l,   11866l,
           11585l,   11297l,   11003l,   10702l,
           10394l,   10080l,    9760l,    9434l,
            9102l,    8765l,    8423l,    8076l,
            7723l,    7366l,    7005l,    6639l,
            6270l,    5897l,    5520l,    5139l,
            4756l,    4370l,    3981l,    3590l,
            3196l,    2801l,    2404l,    2006l,
            1606l,    1205l,     804l,     402l,
               0l,    -401l,    -803l,   -1204l,
           -1605l,   -2005l,   -2403l,   -2800l,
           -3195l,   -3589l,   -3980l,   -4369l,
           -4755l,   -5138l,   -5519l,   -5896l,
           -6269l,   -6638l,   -7004l,   -7365l,
           -7722l,   -8075l,   -8422l,   -8764l,
           -9101l,   -9433l,   -9759l,  -10079l,
          -10393l,  -10701l,  -11002l,  -11296l,
          -11584l,  -11865l,  -12139l,  -12405l,
          -12664l,  -12915l,  -13159l,  -13394l,
          -13622l,  -13841l,  -14052l,  -14255l,
          -14448l,  -14634l,  -14810l,  -14977l,
          -15136l,  -15285l,  -15425l,  -15556l,
          -15678l,  -15790l,  -15892l,  -15985l,
          -16068l,  -16142l,  -16206l,  -16260l,
          -16304l,  -16339l,  -16363l,  -16378l,
          -16383l,
};

#endif

#endif
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

  function: lookup based functions

 ********************************************************************/

#ifndef _V_LOOKUP_H_

#ifdef FLOAT_LOOKUP
extern float vorbis_coslook(float a);
extern float vorbis_invsqlook(float a);
extern float vorbis_invsq2explook(int a);
extern float vorbis_fromdBlook(float a);
#endif
#ifdef INT_LOOKUP
extern long vorbis_invsqlook_i(long a,long e);
extern long vorbis_coslook_i(long a);
extern float vorbis_fromdBlook_i(long a);
#endif

#endif
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2007             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

  function: LPC low level routines

 ********************************************************************/

#ifndef _V_LPC_H_
#define _V_LPC_H_

/*#include "vorbis/codec.h"*/

/* simple linear scale LPC code */
extern float vorbis_lpc_from_data(float *data,float *lpc,int n,int m);

extern void vorbis_lpc_predict(float *coeff,float *prime,int m,
                               float *data,long n);

#endif
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

  function: LSP (also called LSF) conversion routines

 ********************************************************************/


#ifndef _V_LSP_H_
#define _V_LSP_H_

extern int vorbis_lpc_to_lsp(float *lpc,float *lsp,int m);

extern void vorbis_lsp_to_curve(float *curve,int *map,int n,int ln,
                                float *lsp,int m,
                                float amp,float ampoffset);

#endif
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: masking curve data for psychoacoustics

 ********************************************************************/

#ifndef _V_MASKING_H_
#define _V_MASKING_H_

/* more detailed ATH; the bass if flat to save stressing the floor
   overly for only a bin or two of savings. */

#define MAX_ATH 88
static const float ATH[]={
  /*15*/  -51, -52, -53, -54, -55, -56, -57, -58,
  /*31*/  -59, -60, -61, -62, -63, -64, -65, -66,
  /*63*/  -67, -68, -69, -70, -71, -72, -73, -74,
  /*125*/ -75, -76, -77, -78, -80, -81, -82, -83,
  /*250*/ -84, -85, -86, -87, -88, -88, -89, -89,
  /*500*/ -90, -91, -91, -92, -93, -94, -95, -96,
  /*1k*/  -96, -97, -98, -98, -99, -99,-100,-100,
  /*2k*/ -101,-102,-103,-104,-106,-107,-107,-107,
  /*4k*/ -107,-105,-103,-102,-101, -99, -98, -96,
  /*8k*/  -95, -95, -96, -97, -96, -95, -93, -90,
  /*16k*/ -80, -70, -50, -40, -30, -30, -30, -30
};

/* The tone masking curves from Ehmer's and Fielder's papers have been
   replaced by an empirically collected data set.  The previously
   published values were, far too often, simply on crack. */

#define EHMER_OFFSET 16
#define EHMER_MAX 56

/* masking tones from -50 to 0dB, 62.5 through 16kHz at half octaves
   test tones from -2 octaves to +5 octaves sampled at eighth octaves */
/* (Vorbis 0dB, the loudest possible tone, is assumed to be ~100dB SPL
   for collection of these curves) */

static const float tonemasks[P_BANDS][6][EHMER_MAX]={
  /* 62.5 Hz */
  {{ -60,  -60,  -60,  -60,  -60,  -60,  -60,  -60,
     -60,  -60,  -60,  -60,  -62,  -62,  -65,  -73,
     -69,  -68,  -68,  -67,  -70,  -70,  -72,  -74,
     -75,  -79,  -79,  -80,  -83,  -88,  -93, -100,
     -110, -999, -999, -999, -999, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999},
   { -48,  -48,  -48,  -48,  -48,  -48,  -48,  -48,
     -48,  -48,  -48,  -48,  -48,  -53,  -61,  -66,
     -66,  -68,  -67,  -70,  -76,  -76,  -72,  -73,
     -75,  -76,  -78,  -79,  -83,  -88,  -93, -100,
     -110, -999, -999, -999, -999, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999},
   { -37,  -37,  -37,  -37,  -37,  -37,  -37,  -37,
     -38,  -40,  -42,  -46,  -48,  -53,  -55,  -62,
     -65,  -58,  -56,  -56,  -61,  -60,  -65,  -67,
     -69,  -71,  -77,  -77,  -78,  -80,  -82,  -84,
     -88,  -93,  -98, -106, -112, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999},
   { -25,  -25,  -25,  -25,  -25,  -25,  -25,  -25,
     -25,  -26,  -27,  -29,  -32,  -38,  -48,  -52,
     -52,  -50,  -48,  -48,  -51,  -52,  -54,  -60,
     -67,  -67,  -66,  -68,  -69,  -73,  -73,  -76,
     -80,  -81,  -81,  -85,  -85,  -86,  -88,  -93,
     -100, -110, -999, -999, -999, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999},
   { -16,  -16,  -16,  -16,  -16,  -16,  -16,  -16,
     -17,  -19,  -20,  -22,  -26,  -28,  -31,  -40,
     -47,  -39,  -39,  -40,  -42,  -43,  -47,  -51,
     -57,  -52,  -55,  -55,  -60,  -58,  -62,  -63,
     -70,  -67,  -69,  -72,  -73,  -77,  -80,  -82,
     -83,  -87,  -90,  -94,  -98, -104, -115, -999,
     -999, -999, -999, -999, -999, -999, -999, -999},
   {  -8,   -8,   -8,   -8,   -8,   -8,   -8,   -8,
      -8,   -8,  -10,  -11,  -15,  -19,  -25,  -30,
      -34,  -31,  -30,  -31,  -29,  -32,  -35,  -42,
      -48,  -42,  -44,  -46,  -50,  -50,  -51,  -52,
      -59,  -54,  -55,  -55,  -58,  -62,  -63,  -66,
      -72,  -73,  -76,  -75,  -78,  -80,  -80,  -81,
      -84,  -88,  -90,  -94,  -98, -101, -106, -110}},
  /* 88Hz */
  {{ -66,  -66,  -66,  -66,  -66,  -66,  -66,  -66,
     -66,  -66,  -66,  -66,  -66,  -67,  -67,  -67,
     -76,  -72,  -71,  -74,  -76,  -76,  -75,  -78,
     -79,  -79,  -81,  -83,  -86,  -89,  -93,  -97,
     -100, -105, -110, -999, -999, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999},
   { -47,  -47,  -47,  -47,  -47,  -47,  -47,  -47,
     -47,  -47,  -47,  -48,  -51,  -55,  -59,  -66,
     -66,  -66,  -67,  -66,  -68,  -69,  -70,  -74,
     -79,  -77,  -77,  -78,  -80,  -81,  -82,  -84,
     -86,  -88,  -91,  -95, -100, -108, -116, -999,
     -999, -999, -999, -999, -999, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999},
   { -36,  -36,  -36,  -36,  -36,  -36,  -36,  -36,
     -36,  -37,  -37,  -41,  -44,  -48,  -51,  -58,
     -62,  -60,  -57,  -59,  -59,  -60,  -63,  -65,
     -72,  -71,  -70,  -72,  -74,  -77,  -76,  -78,
     -81,  -81,  -80,  -83,  -86,  -91,  -96, -100,
     -105, -110, -999, -999, -999, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999},
   { -28,  -28,  -28,  -28,  -28,  -28,  -28,  -28,
     -28,  -30,  -32,  -32,  -33,  -35,  -41,  -49,
     -50,  -49,  -47,  -48,  -48,  -52,  -51,  -57,
     -65,  -61,  -59,  -61,  -64,  -69,  -70,  -74,
     -77,  -77,  -78,  -81,  -84,  -85,  -87,  -90,
     -92,  -96, -100, -107, -112, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999},
   { -19,  -19,  -19,  -19,  -19,  -19,  -19,  -19,
     -20,  -21,  -23,  -27,  -30,  -35,  -36,  -41,
     -46,  -44,  -42,  -40,  -41,  -41,  -43,  -48,
     -55,  -53,  -52,  -53,  -56,  -59,  -58,  -60,
     -67,  -66,  -69,  -71,  -72,  -75,  -79,  -81,
     -84,  -87,  -90,  -93,  -97, -101, -107, -114,
     -999, -999, -999, -999, -999, -999, -999, -999},
   {  -9,   -9,   -9,   -9,   -9,   -9,   -9,   -9,
      -11,  -12,  -12,  -15,  -16,  -20,  -23,  -30,
      -37,  -34,  -33,  -34,  -31,  -32,  -32,  -38,
      -47,  -44,  -41,  -40,  -47,  -49,  -46,  -46,
      -58,  -50,  -50,  -54,  -58,  -62,  -64,  -67,
      -67,  -70,  -72,  -76,  -79,  -83,  -87,  -91,
      -96, -100, -104, -110, -999, -999, -999, -999}},
  /* 125 Hz */
  {{ -62,  -62,  -62,  -62,  -62,  -62,  -62,  -62,
     -62,  -62,  -63,  -64,  -66,  -67,  -66,  -68,
     -75,  -72,  -76,  -75,  -76,  -78,  -79,  -82,
     -84,  -85,  -90,  -94, -101, -110, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999},
   { -59,  -59,  -59,  -59,  -59,  -59,  -59,  -59,
     -59,  -59,  -59,  -60,  -60,  -61,  -63,  -66,
     -71,  -68,  -70,  -70,  -71,  -72,  -72,  -75,
     -81,  -78,  -79,  -82,  -83,  -86,  -90,  -97,
     -103, -113, -999, -999, -999, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999},
   { -53,  -53,  -53,  -53,  -53,  -53,  -53,  -53,
     -53,  -54,  -55,  -57,  -56,  -57,  -55,  -61,
     -65,  -60,  -60,  -62,  -63,  -63,  -66,  -68,
     -74,  -73,  -75,  -75,  -78,  -80,  -80,  -82,
     -85,  -90,  -96, -101, -108, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999},
   { -46,  -46,  -46,  -46,  -46,  -46,  -46,  -46,
     -46,  -46,  -47,  -47,  -47,  -47,  -48,  -51,
     -57,  -51,  -49,  -50,  -51,  -53,  -54,  -59,
     -66,  -60,  -62,  -67,  -67,  -70,  -72,  -75,
     -76,  -78,  -81,  -85,  -88,  -94,  -97, -104,
     -112, -999, -999, -999, -999, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999},
   { -36,  -36,  -36,  -36,  -36,  -36,  -36,  -36,
     -39,  -41,  -42,  -42,  -39,  -38,  -41,  -43,
     -52,  -44,  -40,  -39,  -37,  -37,  -40,  -47,
     -54,  -50,  -48,  -50,  -55,  -61,  -59,  -62,
     -66,  -66,  -66,  -69,  -69,  -73,  -74,  -74,
     -75,  -77,  -79,  -82,  -87,  -91,  -95, -100,
     -108, -115, -999, -999, -999, -999, -999, -999},
   { -28,  -26,  -24,  -22,  -20,  -20,  -23,  -29,
     -30,  -31,  -28,  -27,  -28,  -28,  -28,  -35,
     -40,  -33,  -32,  -29,  -30,  -30,  -30,  -37,
     -45,  -41,  -37,  -38,  -45,  -47,  -47,  -48,
     -53,  -49,  -48,  -50,  -49,  -49,  -51,  -52,
     -58,  -56,  -57,  -56,  -60,  -61,  -62,  -70,
     -72,  -74,  -78,  -83,  -88,  -93, -100, -106}},
  /* 177 Hz */
  {{-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -110, -105, -100,  -95,  -91,  -87,  -83,
    -80,  -78,  -76,  -78,  -78,  -81,  -83,  -85,
    -86,  -85,  -86,  -87,  -90,  -97, -107, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -110, -105, -100,  -95,  -90,
    -85,  -81,  -77,  -73,  -70,  -67,  -67,  -68,
    -75,  -73,  -70,  -69,  -70,  -72,  -75,  -79,
    -84,  -83,  -84,  -86,  -88,  -89,  -89,  -93,
    -98, -105, -112, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-105, -100,  -95,  -90,  -85,  -80,  -76,  -71,
    -68,  -68,  -65,  -63,  -63,  -62,  -62,  -64,
    -65,  -64,  -61,  -62,  -63,  -64,  -66,  -68,
    -73,  -73,  -74,  -75,  -76,  -81,  -83,  -85,
    -88,  -89,  -92,  -95, -100, -108, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   { -80,  -75,  -71,  -68,  -65,  -63,  -62,  -61,
     -61,  -61,  -61,  -59,  -56,  -57,  -53,  -50,
     -58,  -52,  -50,  -50,  -52,  -53,  -54,  -58,
     -67,  -63,  -67,  -68,  -72,  -75,  -78,  -80,
     -81,  -81,  -82,  -85,  -89,  -90,  -93,  -97,
     -101, -107, -114, -999, -999, -999, -999, -999,
     -999, -999, -999, -999, -999, -999, -999, -999},
   { -65,  -61,  -59,  -57,  -56,  -55,  -55,  -56,
     -56,  -57,  -55,  -53,  -52,  -47,  -44,  -44,
     -50,  -44,  -41,  -39,  -39,  -42,  -40,  -46,
     -51,  -49,  -50,  -53,  -54,  -63,  -60,  -61,
     -62,  -66,  -66,  -66,  -70,  -73,  -74,  -75,
     -76,  -75,  -79,  -85,  -89,  -91,  -96, -102,
     -110, -999, -999, -999, -999, -999, -999, -999},
   { -52,  -50,  -49,  -49,  -48,  -48,  -48,  -49,
     -50,  -50,  -49,  -46,  -43,  -39,  -35,  -33,
     -38,  -36,  -32,  -29,  -32,  -32,  -32,  -35,
     -44,  -39,  -38,  -38,  -46,  -50,  -45,  -46,
     -53,  -50,  -50,  -50,  -54,  -54,  -53,  -53,
     -56,  -57,  -59,  -66,  -70,  -72,  -74,  -79,
     -83,  -85,  -90, -97, -114, -999, -999, -999}},
  /* 250 Hz */
  {{-999, -999, -999, -999, -999, -999, -110, -105,
    -100,  -95,  -90,  -86,  -80,  -75,  -75,  -79,
    -80,  -79,  -80,  -81,  -82,  -88,  -95, -103,
    -110, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -108, -103,  -98,  -93,
    -88,  -83,  -79,  -78,  -75,  -71,  -67,  -68,
    -73,  -73,  -72,  -73,  -75,  -77,  -80,  -82,
    -88,  -93, -100, -107, -114, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -110, -105, -101,  -96,  -90,
    -86,  -81,  -77,  -73,  -69,  -66,  -61,  -62,
    -66,  -64,  -62,  -65,  -66,  -70,  -72,  -76,
    -81,  -80,  -84,  -90,  -95, -102, -110, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -107, -103,  -97,  -92,  -88,
    -83,  -79,  -74,  -70,  -66,  -59,  -53,  -58,
    -62,  -55,  -54,  -54,  -54,  -58,  -61,  -62,
    -72,  -70,  -72,  -75,  -78,  -80,  -81,  -80,
    -83,  -83,  -88,  -93, -100, -107, -115, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -105, -100,  -95,  -90,  -85,
    -80,  -75,  -70,  -66,  -62,  -56,  -48,  -44,
    -48,  -46,  -46,  -43,  -46,  -48,  -48,  -51,
    -58,  -58,  -59,  -60,  -62,  -62,  -61,  -61,
    -65,  -64,  -65,  -68,  -70,  -74,  -75,  -78,
    -81,  -86,  -95, -110, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999,  -999, -105, -100,  -95,  -90,  -85,  -80,
    -75,  -70,  -65,  -61,  -55,  -49,  -39,  -33,
    -40,  -35,  -32,  -38,  -40,  -33,  -35,  -37,
    -46,  -41,  -45,  -44,  -46,  -42,  -45,  -46,
    -52,  -50,  -50,  -50,  -54,  -54,  -55,  -57,
    -62,  -64,  -66,  -68,  -70,  -76,  -81,  -90,
    -100, -110, -999, -999, -999, -999, -999, -999}},
  /* 354 hz */
  {{-999, -999, -999, -999, -999, -999, -999, -999,
    -105,  -98,  -90,  -85,  -82,  -83,  -80,  -78,
    -84,  -79,  -80,  -83,  -87,  -89,  -91,  -93,
    -99, -106, -117, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -105,  -98,  -90,  -85,  -80,  -75,  -70,  -68,
    -74,  -72,  -74,  -77,  -80,  -82,  -85,  -87,
    -92,  -89,  -91,  -95, -100, -106, -112, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -105,  -98,  -90,  -83,  -75,  -71,  -63,  -64,
    -67,  -62,  -64,  -67,  -70,  -73,  -77,  -81,
    -84,  -83,  -85,  -89,  -90,  -93,  -98, -104,
    -109, -114, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -103,  -96,  -88,  -81,  -75,  -68,  -58,  -54,
    -56,  -54,  -56,  -56,  -58,  -60,  -63,  -66,
    -74,  -69,  -72,  -72,  -75,  -74,  -77,  -81,
    -81,  -82,  -84,  -87,  -93,  -96,  -99, -104,
    -110, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -108, -102,  -96,
    -91,  -85,  -80,  -74,  -68,  -60,  -51,  -46,
    -48,  -46,  -43,  -45,  -47,  -47,  -49,  -48,
    -56,  -53,  -55,  -58,  -57,  -63,  -58,  -60,
    -66,  -64,  -67,  -70,  -70,  -74,  -77,  -84,
    -86,  -89,  -91,  -93,  -94, -101, -109, -118,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -108, -103,  -98,  -93,  -88,
    -83,  -78,  -73,  -68,  -60,  -53,  -44,  -35,
    -38,  -38,  -34,  -34,  -36,  -40,  -41,  -44,
    -51,  -45,  -46,  -47,  -46,  -54,  -50,  -49,
    -50,  -50,  -50,  -51,  -54,  -57,  -58,  -60,
    -66,  -66,  -66,  -64,  -65,  -68,  -77,  -82,
    -87,  -95, -110, -999, -999, -999, -999, -999}},
  /* 500 Hz */
  {{-999, -999, -999, -999, -999, -999, -999, -999,
    -107, -102,  -97,  -92,  -87,  -83,  -78,  -75,
    -82,  -79,  -83,  -85,  -89,  -92,  -95,  -98,
    -101, -105, -109, -113, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -106,
    -100,  -95,  -90,  -86,  -81,  -78,  -74,  -69,
    -74,  -74,  -76,  -79,  -83,  -84,  -86,  -89,
    -92,  -97,  -93, -100, -103, -107, -110, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -106, -100,
    -95, -90, -87, -83, -80, -75, -69, -60,
    -66, -66, -68, -70, -74, -78, -79, -81,
    -81, -83, -84, -87, -93, -96, -99, -103,
    -107, -110, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -108, -103, -98,
    -93, -89, -85, -82, -78, -71, -62, -55,
    -58, -58, -54, -54, -55, -59, -61, -62,
    -70, -66, -66, -67, -70, -72, -75, -78,
    -84, -84, -84, -88, -91, -90, -95, -98,
    -102, -103, -106, -110, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -108, -103,  -98,  -94,
    -90,  -87,  -82,  -79,  -73,  -67,  -58,  -47,
    -50,  -45,  -41,  -45,  -48,  -44,  -44,  -49,
    -54,  -51,  -48,  -47,  -49,  -50,  -51,  -57,
    -58,  -60,  -63,  -69,  -70,  -69,  -71,  -74,
    -78,  -82,  -90,  -95, -101, -105, -110, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -105, -101, -97, -93, -90,
    -85, -80, -77, -72, -65, -56, -48, -37,
    -40, -36, -34, -40, -50, -47, -38, -41,
    -47, -38, -35, -39, -38, -43, -40, -45,
    -50, -45, -44, -47, -50, -55, -48, -48,
    -52, -66, -70, -76, -82, -90, -97, -105,
    -110, -999, -999, -999, -999, -999, -999, -999}},
  /* 707 Hz */
  {{-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -108, -103,  -98,  -93,  -86,  -79,  -76,
    -83,  -81,  -85,  -87,  -89,  -93,  -98, -102,
    -107, -112, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -108, -103,  -98,  -93,  -86,  -79,  -71,
    -77,  -74,  -77,  -79,  -81,  -84,  -85,  -90,
    -92,  -93,  -92,  -98, -101, -108, -112, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -108, -103,  -98,  -93,  -87,  -78,  -68,  -65,
    -66,  -62,  -65,  -67,  -70,  -73,  -75,  -78,
    -82,  -82,  -83,  -84,  -91,  -93,  -98, -102,
    -106, -110, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -105, -100, -95, -90, -82, -74, -62, -57,
    -58, -56, -51, -52, -52, -54, -54, -58,
    -66, -59, -60, -63, -66, -69, -73, -79,
    -83, -84, -80, -81, -81, -82, -88, -92,
    -98, -105, -113, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -107,
    -102,  -97,  -92,  -84,  -79,  -69,  -57,  -47,
    -52,  -47,  -44,  -45,  -50,  -52,  -42,  -42,
    -53,  -43,  -43,  -48,  -51,  -56,  -55,  -52,
    -57,  -59,  -61,  -62,  -67,  -71,  -78,  -83,
    -86,  -94,  -98, -103, -110, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -105, -100,
    -95,  -90,  -84,  -78,  -70,  -61,  -51,  -41,
    -40,  -38,  -40,  -46,  -52,  -51,  -41,  -40,
    -46,  -40,  -38,  -38,  -41,  -46,  -41,  -46,
    -47,  -43,  -43,  -45,  -41,  -45,  -56,  -67,
    -68,  -83,  -87,  -90,  -95, -102, -107, -113,
    -999, -999, -999, -999, -999, -999, -999, -999}},
  /* 1000 Hz */
  {{-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -109, -105, -101,  -96,  -91,  -84,  -77,
    -82,  -82,  -85,  -89,  -94, -100, -106, -110,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -106, -103,  -98,  -92,  -85,  -80,  -71,
    -75,  -72,  -76,  -80,  -84,  -86,  -89,  -93,
    -100, -107, -113, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -107,
    -104, -101,  -97,  -92,  -88,  -84,  -80,  -64,
    -66,  -63,  -64,  -66,  -69,  -73,  -77,  -83,
    -83,  -86,  -91,  -98, -104, -111, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -107,
    -104, -101,  -97,  -92,  -90,  -84,  -74,  -57,
    -58,  -52,  -55,  -54,  -50,  -52,  -50,  -52,
    -63,  -62,  -69,  -76,  -77,  -78,  -78,  -79,
    -82,  -88,  -94, -100, -106, -111, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -106, -102,
    -98,  -95,  -90,  -85,  -83,  -78,  -70,  -50,
    -50,  -41,  -44,  -49,  -47,  -50,  -50,  -44,
    -55,  -46,  -47,  -48,  -48,  -54,  -49,  -49,
    -58,  -62,  -71,  -81,  -87,  -92,  -97, -102,
    -108, -114, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -106, -102,
    -98,  -95,  -90,  -85,  -83,  -78,  -70,  -45,
    -43,  -41,  -47,  -50,  -51,  -50,  -49,  -45,
    -47,  -41,  -44,  -41,  -39,  -43,  -38,  -37,
    -40,  -41,  -44,  -50,  -58,  -65,  -73,  -79,
    -85,  -92,  -97, -101, -105, -109, -113, -999,
    -999, -999, -999, -999, -999, -999, -999, -999}},
  /* 1414 Hz */
  {{-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -107, -100,  -95,  -87,  -81,
    -85,  -83,  -88,  -93, -100, -107, -114, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -107, -101,  -95,  -88,  -83,  -76,
    -73,  -72,  -79,  -84,  -90,  -95, -100, -105,
    -110, -115, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -104,  -98,  -92,  -87,  -81,  -70,
    -65,  -62,  -67,  -71,  -74,  -80,  -85,  -91,
    -95,  -99, -103, -108, -111, -114, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -103,  -97,  -90,  -85,  -76,  -60,
    -56,  -54,  -60,  -62,  -61,  -56,  -63,  -65,
    -73,  -74,  -77,  -75,  -78,  -81,  -86,  -87,
    -88,  -91,  -94,  -98, -103, -110, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -105,
    -100,  -97,  -92,  -86,  -81,  -79,  -70,  -57,
    -51,  -47,  -51,  -58,  -60,  -56,  -53,  -50,
    -58,  -52,  -50,  -50,  -53,  -55,  -64,  -69,
    -71,  -85,  -82,  -78,  -81,  -85,  -95, -102,
    -112, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -105,
    -100,  -97,  -92,  -85,  -83,  -79,  -72,  -49,
    -40,  -43,  -43,  -54,  -56,  -51,  -50,  -40,
    -43,  -38,  -36,  -35,  -37,  -38,  -37,  -44,
    -54,  -60,  -57,  -60,  -70,  -75,  -84,  -92,
    -103, -112, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999}},
  /* 2000 Hz */
  {{-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -110, -102,  -95,  -89,  -82,
    -83,  -84,  -90,  -92,  -99, -107, -113, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -107, -101,  -95,  -89,  -83,  -72,
    -74,  -78,  -85,  -88,  -88,  -90,  -92,  -98,
    -105, -111, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -109, -103, -97, -93, -87, -81, -70,
    -70, -67, -75, -73, -76, -79, -81, -83,
    -88, -89, -97, -103, -110, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -107, -100,  -94,  -88,  -83,  -75,  -63,
    -59,  -59,  -63,  -66,  -60,  -62,  -67,  -67,
    -77,  -76,  -81,  -88,  -86,  -92,  -96, -102,
    -109, -116, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -105,  -98,  -92,  -86,  -81,  -73,  -56,
    -52,  -47,  -55,  -60,  -58,  -52,  -51,  -45,
    -49,  -50,  -53,  -54,  -61,  -71,  -70,  -69,
    -78,  -79,  -87,  -90,  -96, -104, -112, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -103,  -96,  -90,  -86,  -78,  -70,  -51,
    -42,  -47,  -48,  -55,  -54,  -54,  -53,  -42,
    -35,  -28,  -33,  -38,  -37,  -44,  -47,  -49,
    -54,  -63,  -68,  -78,  -82,  -89,  -94,  -99,
    -104, -109, -114, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999}},
  /* 2828 Hz */
  {{-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -110, -100,  -90,  -79,
    -85,  -81,  -82,  -82,  -89,  -94,  -99, -103,
    -109, -115, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -105,  -97,  -85,  -72,
    -74,  -70,  -70,  -70,  -76,  -85,  -91,  -93,
    -97, -103, -109, -115, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -112,  -93,  -81,  -68,
    -62,  -60,  -60,  -57,  -63,  -70,  -77,  -82,
    -90,  -93,  -98, -104, -109, -113, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -113, -100,  -93,  -84,  -63,
    -58,  -48,  -53,  -54,  -52,  -52,  -57,  -64,
    -66,  -76,  -83,  -81,  -85,  -85,  -90,  -95,
    -98, -101, -103, -106, -108, -111, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -105,  -95,  -86,  -74,  -53,
    -50,  -38,  -43,  -49,  -43,  -42,  -39,  -39,
    -46,  -52,  -57,  -56,  -72,  -69,  -74,  -81,
    -87,  -92,  -94,  -97,  -99, -102, -105, -108,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -108,  -99,  -90,  -76,  -66,  -45,
    -43,  -41,  -44,  -47,  -43,  -47,  -40,  -30,
    -31,  -31,  -39,  -33,  -40,  -41,  -43,  -53,
    -59,  -70,  -73,  -77,  -79,  -82,  -84,  -87,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999}},
  /* 4000 Hz */
  {{-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -110,  -91,  -76,
    -75,  -85,  -93,  -98, -104, -110, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -110,  -91,  -70,
    -70,  -75,  -86,  -89,  -94,  -98, -101, -106,
    -110, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -110,  -95,  -80,  -60,
    -65,  -64,  -74,  -83,  -88,  -91,  -95,  -99,
    -103, -107, -110, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -110,  -95,  -80,  -58,
    -55,  -49,  -66,  -68,  -71,  -78,  -78,  -80,
    -88,  -85,  -89,  -97, -100, -105, -110, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -110,  -95,  -80,  -53,
    -52,  -41,  -59,  -59,  -49,  -58,  -56,  -63,
    -86,  -79,  -90,  -93,  -98, -103, -107, -112,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -110,  -97,  -91,  -73,  -45,
    -40,  -33,  -53,  -61,  -49,  -54,  -50,  -50,
    -60,  -52,  -67,  -74,  -81,  -92,  -96, -100,
    -105, -110, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999}},
  /* 5657 Hz */
  {{-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -113, -106,  -99,  -92,  -77,
    -80,  -88,  -97, -106, -115, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -116, -109, -102,  -95,  -89,  -74,
    -72,  -88,  -87,  -95, -102, -109, -116, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -116, -109, -102,  -95,  -89,  -75,
    -66,  -74,  -77,  -78,  -86,  -87,  -90,  -96,
    -105, -115, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -115, -108, -101,  -94,  -88,  -66,
    -56,  -61,  -70,  -65,  -78,  -72,  -83,  -84,
    -93,  -98, -105, -110, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -110, -105,  -95,  -89,  -82,  -57,
    -52,  -52,  -59,  -56,  -59,  -58,  -69,  -67,
    -88,  -82,  -82,  -89,  -94, -100, -108, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -110, -101,  -96,  -90,  -83,  -77,  -54,
    -43,  -38,  -50,  -48,  -52,  -48,  -42,  -42,
    -51,  -52,  -53,  -59,  -65,  -71,  -78,  -85,
    -95, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999}},
  /* 8000 Hz */
  {{-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -120, -105,  -86,  -68,
    -78,  -79,  -90, -100, -110, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -120, -105,  -86,  -66,
    -73,  -77,  -88,  -96, -105, -115, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -120, -105,  -92,  -80,  -61,
    -64,  -68,  -80,  -87,  -92, -100, -110, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -120, -104,  -91,  -79,  -52,
    -60,  -54,  -64,  -69,  -77,  -80,  -82,  -84,
    -85,  -87,  -88,  -90, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -118, -100,  -87,  -77,  -49,
    -50,  -44,  -58,  -61,  -61,  -67,  -65,  -62,
    -62,  -62,  -65,  -68, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -115,  -98,  -84,  -62,  -49,
    -44,  -38,  -46,  -49,  -49,  -46,  -39,  -37,
    -39,  -40,  -42,  -43, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999}},
  /* 11314 Hz */
  {{-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -110,  -88,  -74,
    -77,  -82,  -82,  -85,  -90,  -94,  -99, -104,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -110,  -88,  -66,
    -70,  -81,  -80,  -81,  -84,  -88,  -91,  -93,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -110,  -88,  -61,
    -63,  -70,  -71,  -74,  -77,  -80,  -83,  -85,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -110, -86, -62,
    -63,  -62,  -62,  -58,  -52,  -50,  -50,  -52,
    -54, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -118, -108,  -84,  -53,
    -50,  -50,  -50,  -55,  -47,  -45,  -40,  -40,
    -40, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -118, -100,  -73,  -43,
    -37,  -42,  -43,  -53,  -38,  -37,  -35,  -35,
    -38, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999}},
  /* 16000 Hz */
  {{-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -110, -100,  -91,  -84,  -74,
    -80,  -80,  -80,  -80,  -80, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -110, -100,  -91,  -84,  -74,
    -68,  -68,  -68,  -68,  -68, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -110, -100,  -86,  -78,  -70,
    -60,  -45,  -30,  -21, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -110, -100,  -87,  -78,  -67,
    -48,  -38,  -29,  -21, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -110, -100,  -86,  -69,  -56,
    -45,  -35,  -33,  -29, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999},
   {-999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -110, -100,  -83,  -71,  -48,
    -27,  -38,  -37,  -34, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999,
    -999, -999, -999, -999, -999, -999, -999, -999}}
};

#endif
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2007             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: registry for time, floor, res backends and channel mappings

 ********************************************************************/

#ifndef _V_REG_H_
#define _V_REG_H_

#define VI_TRANSFORMB 1
#define VI_WINDOWB 1
#define VI_TIMEB 1
#define VI_FLOORB 2
#define VI_RESB 3
#define VI_MAPB 1

extern const vorbis_func_floor     *const _floor_P[];
extern const vorbis_func_residue   *const _residue_P[];
extern const vorbis_func_mapping   *const _mapping_P[];

#endif
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: linear scale -> dB, Bark and Mel scales

 ********************************************************************/

#ifndef _V_SCALES_H_
#define _V_SCALES_H_

#include <math.h>
/*#include "os.h"*/

#ifdef _MSC_VER
/* MS Visual Studio doesn't have C99 inline keyword. */
#define inline __inline
#endif

/* 20log10(x) */
#define VORBIS_IEEE_FLOAT32 1
#ifdef VORBIS_IEEE_FLOAT32

static inline float unitnorm(float x){
  union {
    ogg_uint32_t i;
    float f;
  } ix;
  ix.f = x;
  ix.i = (ix.i & 0x80000000U) | (0x3f800000U);
  return ix.f;
}

/* Segher was off (too high) by ~ .3 decibel.  Center the conversion correctly. */
static inline float todB(const float *x){
  union {
    ogg_uint32_t i;
    float f;
  } ix;
  ix.f = *x;
  ix.i = ix.i&0x7fffffff;
  return (float)(ix.i * 7.17711438e-7f -764.6161886f);
}

#define todB_nn(x) todB(x)

#else

static float unitnorm(float x){
  if(x<0)return(-1.f);
  return(1.f);
}

#define todB(x)   (*(x)==0?-400.f:log(*(x)**(x))*4.34294480f)
#define todB_nn(x)   (*(x)==0.f?-400.f:log(*(x))*8.6858896f)

#endif

#define fromdB(x) (exp((x)*.11512925f))

/* The bark scale equations are approximations, since the original
   table was somewhat hand rolled.  The below are chosen to have the
   best possible fit to the rolled tables, thus their somewhat odd
   appearance (these are more accurate and over a longer range than
   the oft-quoted bark equations found in the texts I have).  The
   approximations are valid from 0 - 30kHz (nyquist) or so.

   all f in Hz, z in Bark */

#define toBARK(n)   (13.1f*atan(.00074f*(n))+2.24f*atan((n)*(n)*1.85e-8f)+1e-4f*(n))
#define fromBARK(z) (102.f*(z)-2.f*pow(z,2.f)+.4f*pow(z,3.f)+pow(1.46f,z)-1.f)
#define toMEL(n)    (log(1.f+(n)*.001f)*1442.695f)
#define fromMEL(m)  (1000.f*exp((m)/1442.695f)-1000.f)

/* Frequency to octave.  We arbitrarily declare 63.5 Hz to be octave
   0.0 */

#define toOC(n)     (log(n)*1.442695f-5.965784f)
#define fromOC(o)   (exp(((o)+5.965784f)*.693147f))

#endif
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2007             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: window functions

 ********************************************************************/

#ifndef _V_WINDOW_
#define _V_WINDOW_

extern const float *_vorbis_window_get(int n);
extern void _vorbis_apply_window(float *d,int *winno,long *blocksizes,
                          int lW,int W,int nW);


#endif
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: normalized modified discrete cosine transform
           power of two length transform only [64 <= n ]

 Original algorithm adapted long ago from _The use of multirate filter
 banks for coding of high quality digital audio_, by T. Sporer,
 K. Brandenburg and B. Edler, collection of the European Signal
 Processing Conference (EUSIPCO), Amsterdam, June 1992, Vol.1, pp
 211-214

 The below code implements an algorithm that no longer looks much like
 that presented in the paper, but the basic structure remains if you
 dig deep enough to see it.

 This module DOES NOT INCLUDE code to generate/apply the window
 function.  Everybody has their own weird favorite including me... I
 happen to like the properties of y=sin(.5PI*sin^2(x)), but others may
 vehemently disagree.

 ********************************************************************/

/* this can also be run as an integer transform by uncommenting a
   define in mdct.h; the integerization is a first pass and although
   it's likely stable for Vorbis, the dynamic range is constrained and
   roundoff isn't done (so it's noisy).  Consider it functional, but
   only a starting point.  There's no point on a machine with an FPU */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
/*#include "vorbis/codec.h"*/
/*#include "mdct.h"*/
/*#include "os.h"*/
/*#include "misc.h"*/

/* build lookups for trig functions; also pre-figure scaling and
   some window function algebra. */

void mdct_init(mdct_lookup *lookup,int n){
  int   *bitrev=(int*)_ogg_malloc(sizeof(*bitrev)*(n/4));
  DATA_TYPE *T=(DATA_TYPE*)_ogg_malloc(sizeof(*T)*(n+n/4));

  int i;
  int n2=n>>1;
  int log2n=lookup->log2n=rint(log((float)n)/log(2.f));
  lookup->n=n;
  lookup->trig=T;
  lookup->bitrev=bitrev;

/* trig lookups... */

  for(i=0;i<n/4;i++){
    T[i*2]=FLOAT_CONV(cos((M_PI/n)*(4*i)));
    T[i*2+1]=FLOAT_CONV(-sin((M_PI/n)*(4*i)));
    T[n2+i*2]=FLOAT_CONV(cos((M_PI/(2*n))*(2*i+1)));
    T[n2+i*2+1]=FLOAT_CONV(sin((M_PI/(2*n))*(2*i+1)));
  }
  for(i=0;i<n/8;i++){
    T[n+i*2]=FLOAT_CONV(cos((M_PI/n)*(4*i+2))*.5);
    T[n+i*2+1]=FLOAT_CONV(-sin((M_PI/n)*(4*i+2))*.5);
  }

  /* _bitreverse lookup... */

  {
    int mask=(1<<(log2n-1))-1,i,j;
    int msb=1<<(log2n-2);
    for(i=0;i<n/8;i++){
      int acc=0;
      for(j=0;msb>>j;j++)
        if((msb>>j)&i)acc|=1<<j;
      bitrev[i*2]=((~acc)&mask)-1;
      bitrev[i*2+1]=acc;

    }
  }
  lookup->scale=FLOAT_CONV(4.f/n);
}

/* 8 point butterfly (in place, 4 register) */
STIN void mdct_butterfly_8(DATA_TYPE *x){
  REG_TYPE r0   = x[6] + x[2];
  REG_TYPE r1   = x[6] - x[2];
  REG_TYPE r2   = x[4] + x[0];
  REG_TYPE r3   = x[4] - x[0];

           x[6] = r0   + r2;
           x[4] = r0   - r2;

           r0   = x[5] - x[1];
           r2   = x[7] - x[3];
           x[0] = r1   + r0;
           x[2] = r1   - r0;

           r0   = x[5] + x[1];
           r1   = x[7] + x[3];
           x[3] = r2   + r3;
           x[1] = r2   - r3;
           x[7] = r1   + r0;
           x[5] = r1   - r0;

}

/* 16 point butterfly (in place, 4 register) */
STIN void mdct_butterfly_16(DATA_TYPE *x){
  REG_TYPE r0     = x[1]  - x[9];
  REG_TYPE r1     = x[0]  - x[8];

           x[8]  += x[0];
           x[9]  += x[1];
           x[0]   = MULT_NORM((r0   + r1) * cPI2_8);
           x[1]   = MULT_NORM((r0   - r1) * cPI2_8);

           r0     = x[3]  - x[11];
           r1     = x[10] - x[2];
           x[10] += x[2];
           x[11] += x[3];
           x[2]   = r0;
           x[3]   = r1;

           r0     = x[12] - x[4];
           r1     = x[13] - x[5];
           x[12] += x[4];
           x[13] += x[5];
           x[4]   = MULT_NORM((r0   - r1) * cPI2_8);
           x[5]   = MULT_NORM((r0   + r1) * cPI2_8);

           r0     = x[14] - x[6];
           r1     = x[15] - x[7];
           x[14] += x[6];
           x[15] += x[7];
           x[6]  = r0;
           x[7]  = r1;

           mdct_butterfly_8(x);
           mdct_butterfly_8(x+8);
}

/* 32 point butterfly (in place, 4 register) */
STIN void mdct_butterfly_32(DATA_TYPE *x){
  REG_TYPE r0     = x[30] - x[14];
  REG_TYPE r1     = x[31] - x[15];

           x[30] +=         x[14];
           x[31] +=         x[15];
           x[14]  =         r0;
           x[15]  =         r1;

           r0     = x[28] - x[12];
           r1     = x[29] - x[13];
           x[28] +=         x[12];
           x[29] +=         x[13];
           x[12]  = MULT_NORM( r0 * cPI1_8  -  r1 * cPI3_8 );
           x[13]  = MULT_NORM( r0 * cPI3_8  +  r1 * cPI1_8 );

           r0     = x[26] - x[10];
           r1     = x[27] - x[11];
           x[26] +=         x[10];
           x[27] +=         x[11];
           x[10]  = MULT_NORM(( r0  - r1 ) * cPI2_8);
           x[11]  = MULT_NORM(( r0  + r1 ) * cPI2_8);

           r0     = x[24] - x[8];
           r1     = x[25] - x[9];
           x[24] += x[8];
           x[25] += x[9];
           x[8]   = MULT_NORM( r0 * cPI3_8  -  r1 * cPI1_8 );
           x[9]   = MULT_NORM( r1 * cPI3_8  +  r0 * cPI1_8 );

           r0     = x[22] - x[6];
           r1     = x[7]  - x[23];
           x[22] += x[6];
           x[23] += x[7];
           x[6]   = r1;
           x[7]   = r0;

           r0     = x[4]  - x[20];
           r1     = x[5]  - x[21];
           x[20] += x[4];
           x[21] += x[5];
           x[4]   = MULT_NORM( r1 * cPI1_8  +  r0 * cPI3_8 );
           x[5]   = MULT_NORM( r1 * cPI3_8  -  r0 * cPI1_8 );

           r0     = x[2]  - x[18];
           r1     = x[3]  - x[19];
           x[18] += x[2];
           x[19] += x[3];
           x[2]   = MULT_NORM(( r1  + r0 ) * cPI2_8);
           x[3]   = MULT_NORM(( r1  - r0 ) * cPI2_8);

           r0     = x[0]  - x[16];
           r1     = x[1]  - x[17];
           x[16] += x[0];
           x[17] += x[1];
           x[0]   = MULT_NORM( r1 * cPI3_8  +  r0 * cPI1_8 );
           x[1]   = MULT_NORM( r1 * cPI1_8  -  r0 * cPI3_8 );

           mdct_butterfly_16(x);
           mdct_butterfly_16(x+16);

}

/* N point first stage butterfly (in place, 2 register) */
STIN void mdct_butterfly_first(DATA_TYPE *T,
                                        DATA_TYPE *x,
                                        int points){

  DATA_TYPE *x1        = x          + points      - 8;
  DATA_TYPE *x2        = x          + (points>>1) - 8;
  REG_TYPE   r0;
  REG_TYPE   r1;

  do{

               r0      = x1[6]      -  x2[6];
               r1      = x1[7]      -  x2[7];
               x1[6]  += x2[6];
               x1[7]  += x2[7];
               x2[6]   = MULT_NORM(r1 * T[1]  +  r0 * T[0]);
               x2[7]   = MULT_NORM(r1 * T[0]  -  r0 * T[1]);

               r0      = x1[4]      -  x2[4];
               r1      = x1[5]      -  x2[5];
               x1[4]  += x2[4];
               x1[5]  += x2[5];
               x2[4]   = MULT_NORM(r1 * T[5]  +  r0 * T[4]);
               x2[5]   = MULT_NORM(r1 * T[4]  -  r0 * T[5]);

               r0      = x1[2]      -  x2[2];
               r1      = x1[3]      -  x2[3];
               x1[2]  += x2[2];
               x1[3]  += x2[3];
               x2[2]   = MULT_NORM(r1 * T[9]  +  r0 * T[8]);
               x2[3]   = MULT_NORM(r1 * T[8]  -  r0 * T[9]);

               r0      = x1[0]      -  x2[0];
               r1      = x1[1]      -  x2[1];
               x1[0]  += x2[0];
               x1[1]  += x2[1];
               x2[0]   = MULT_NORM(r1 * T[13] +  r0 * T[12]);
               x2[1]   = MULT_NORM(r1 * T[12] -  r0 * T[13]);

    x1-=8;
    x2-=8;
    T+=16;

  }while(x2>=x);
}

/* N/stage point generic N stage butterfly (in place, 2 register) */
STIN void mdct_butterfly_generic(DATA_TYPE *T,
                                          DATA_TYPE *x,
                                          int points,
                                          int trigint){

  DATA_TYPE *x1        = x          + points      - 8;
  DATA_TYPE *x2        = x          + (points>>1) - 8;
  REG_TYPE   r0;
  REG_TYPE   r1;

  do{

               r0      = x1[6]      -  x2[6];
               r1      = x1[7]      -  x2[7];
               x1[6]  += x2[6];
               x1[7]  += x2[7];
               x2[6]   = MULT_NORM(r1 * T[1]  +  r0 * T[0]);
               x2[7]   = MULT_NORM(r1 * T[0]  -  r0 * T[1]);

               T+=trigint;

               r0      = x1[4]      -  x2[4];
               r1      = x1[5]      -  x2[5];
               x1[4]  += x2[4];
               x1[5]  += x2[5];
               x2[4]   = MULT_NORM(r1 * T[1]  +  r0 * T[0]);
               x2[5]   = MULT_NORM(r1 * T[0]  -  r0 * T[1]);

               T+=trigint;

               r0      = x1[2]      -  x2[2];
               r1      = x1[3]      -  x2[3];
               x1[2]  += x2[2];
               x1[3]  += x2[3];
               x2[2]   = MULT_NORM(r1 * T[1]  +  r0 * T[0]);
               x2[3]   = MULT_NORM(r1 * T[0]  -  r0 * T[1]);

               T+=trigint;

               r0      = x1[0]      -  x2[0];
               r1      = x1[1]      -  x2[1];
               x1[0]  += x2[0];
               x1[1]  += x2[1];
               x2[0]   = MULT_NORM(r1 * T[1]  +  r0 * T[0]);
               x2[1]   = MULT_NORM(r1 * T[0]  -  r0 * T[1]);

               T+=trigint;
    x1-=8;
    x2-=8;

  }while(x2>=x);
}

STIN void mdct_butterflies(mdct_lookup *init,
                             DATA_TYPE *x,
                             int points){

  DATA_TYPE *T=init->trig;
  int stages=init->log2n-5;
  int i,j;

  if(--stages>0){
    mdct_butterfly_first(T,x,points);
  }

  for(i=1;--stages>0;i++){
    for(j=0;j<(1<<i);j++)
      mdct_butterfly_generic(T,x+(points>>i)*j,points>>i,4<<i);
  }

  for(j=0;j<points;j+=32)
    mdct_butterfly_32(x+j);

}

void mdct_clear(mdct_lookup *l){
  if(l){
    if(l->trig)_ogg_free(l->trig);
    if(l->bitrev)_ogg_free(l->bitrev);
    memset(l,0,sizeof(*l));
  }
}

STIN void mdct__bitreverse(mdct_lookup *init,
                            DATA_TYPE *x){
  int        n       = init->n;
  int       *bit     = init->bitrev;
  DATA_TYPE *w0      = x;
  DATA_TYPE *w1      = x = w0+(n>>1);
  DATA_TYPE *T       = init->trig+n;

  do{
    DATA_TYPE *x0    = x+bit[0];
    DATA_TYPE *x1    = x+bit[1];

    REG_TYPE  r0     = x0[1]  - x1[1];
    REG_TYPE  r1     = x0[0]  + x1[0];
    REG_TYPE  r2     = MULT_NORM(r1     * T[0]   + r0 * T[1]);
    REG_TYPE  r3     = MULT_NORM(r1     * T[1]   - r0 * T[0]);

              w1    -= 4;

              r0     = HALVE(x0[1] + x1[1]);
              r1     = HALVE(x0[0] - x1[0]);

              w0[0]  = r0     + r2;
              w1[2]  = r0     - r2;
              w0[1]  = r1     + r3;
              w1[3]  = r3     - r1;

              x0     = x+bit[2];
              x1     = x+bit[3];

              r0     = x0[1]  - x1[1];
              r1     = x0[0]  + x1[0];
              r2     = MULT_NORM(r1     * T[2]   + r0 * T[3]);
              r3     = MULT_NORM(r1     * T[3]   - r0 * T[2]);

              r0     = HALVE(x0[1] + x1[1]);
              r1     = HALVE(x0[0] - x1[0]);

              w0[2]  = r0     + r2;
              w1[0]  = r0     - r2;
              w0[3]  = r1     + r3;
              w1[1]  = r3     - r1;

              T     += 4;
              bit   += 4;
              w0    += 4;

  }while(w0<w1);
}

void mdct_backward(mdct_lookup *init, DATA_TYPE *in, DATA_TYPE *out){
  int n=init->n;
  int n2=n>>1;
  int n4=n>>2;

  /* rotate */

  DATA_TYPE *iX = in+n2-7;
  DATA_TYPE *oX = out+n2+n4;
  DATA_TYPE *T  = init->trig+n4;

  do{
    oX         -= 4;
    oX[0]       = MULT_NORM(-iX[2] * T[3] - iX[0]  * T[2]);
    oX[1]       = MULT_NORM (iX[0] * T[3] - iX[2]  * T[2]);
    oX[2]       = MULT_NORM(-iX[6] * T[1] - iX[4]  * T[0]);
    oX[3]       = MULT_NORM (iX[4] * T[1] - iX[6]  * T[0]);
    iX         -= 8;
    T          += 4;
  }while(iX>=in);

  iX            = in+n2-8;
  oX            = out+n2+n4;
  T             = init->trig+n4;

  do{
    T          -= 4;
    oX[0]       =  MULT_NORM (iX[4] * T[3] + iX[6] * T[2]);
    oX[1]       =  MULT_NORM (iX[4] * T[2] - iX[6] * T[3]);
    oX[2]       =  MULT_NORM (iX[0] * T[1] + iX[2] * T[0]);
    oX[3]       =  MULT_NORM (iX[0] * T[0] - iX[2] * T[1]);
    iX         -= 8;
    oX         += 4;
  }while(iX>=in);

  mdct_butterflies(init,out+n2,n2);
  mdct__bitreverse(init,out);

  /* roatate + window */

  {
    DATA_TYPE *oX1=out+n2+n4;
    DATA_TYPE *oX2=out+n2+n4;
    DATA_TYPE *iX =out;
    T             =init->trig+n2;

    do{
      oX1-=4;

      oX1[3]  =  MULT_NORM (iX[0] * T[1] - iX[1] * T[0]);
      oX2[0]  = -MULT_NORM (iX[0] * T[0] + iX[1] * T[1]);

      oX1[2]  =  MULT_NORM (iX[2] * T[3] - iX[3] * T[2]);
      oX2[1]  = -MULT_NORM (iX[2] * T[2] + iX[3] * T[3]);

      oX1[1]  =  MULT_NORM (iX[4] * T[5] - iX[5] * T[4]);
      oX2[2]  = -MULT_NORM (iX[4] * T[4] + iX[5] * T[5]);

      oX1[0]  =  MULT_NORM (iX[6] * T[7] - iX[7] * T[6]);
      oX2[3]  = -MULT_NORM (iX[6] * T[6] + iX[7] * T[7]);

      oX2+=4;
      iX    +=   8;
      T     +=   8;
    }while(iX<oX1);

    iX=out+n2+n4;
    oX1=out+n4;
    oX2=oX1;

    do{
      oX1-=4;
      iX-=4;

      oX2[0] = -(oX1[3] = iX[3]);
      oX2[1] = -(oX1[2] = iX[2]);
      oX2[2] = -(oX1[1] = iX[1]);
      oX2[3] = -(oX1[0] = iX[0]);

      oX2+=4;
    }while(oX2<iX);

    iX=out+n2+n4;
    oX1=out+n2+n4;
    oX2=out+n2;
    do{
      oX1-=4;
      oX1[0]= iX[3];
      oX1[1]= iX[2];
      oX1[2]= iX[1];
      oX1[3]= iX[0];
      iX+=4;
    }while(oX1>oX2);
  }
}

void mdct_forward(mdct_lookup *init, DATA_TYPE *in, DATA_TYPE *out){
  int n=init->n;
  int n2=n>>1;
  int n4=n>>2;
  int n8=n>>3;
  DATA_TYPE *w=(DATA_TYPE*)alloca(n*sizeof(*w)); /* forward needs working space */
  DATA_TYPE *w2=w+n2;

  /* rotate */

  /* window + rotate + step 1 */

  REG_TYPE r0;
  REG_TYPE r1;
  DATA_TYPE *x0=in+n2+n4;
  DATA_TYPE *x1=x0+1;
  DATA_TYPE *T=init->trig+n2;

  int i=0;

  for(i=0;i<n8;i+=2){
    x0 -=4;
    T-=2;
    r0= x0[2] + x1[0];
    r1= x0[0] + x1[2];
    w2[i]=   MULT_NORM(r1*T[1] + r0*T[0]);
    w2[i+1]= MULT_NORM(r1*T[0] - r0*T[1]);
    x1 +=4;
  }

  x1=in+1;

  for(;i<n2-n8;i+=2){
    T-=2;
    x0 -=4;
    r0= x0[2] - x1[0];
    r1= x0[0] - x1[2];
    w2[i]=   MULT_NORM(r1*T[1] + r0*T[0]);
    w2[i+1]= MULT_NORM(r1*T[0] - r0*T[1]);
    x1 +=4;
  }

  x0=in+n;

  for(;i<n2;i+=2){
    T-=2;
    x0 -=4;
    r0= -x0[2] - x1[0];
    r1= -x0[0] - x1[2];
    w2[i]=   MULT_NORM(r1*T[1] + r0*T[0]);
    w2[i+1]= MULT_NORM(r1*T[0] - r0*T[1]);
    x1 +=4;
  }


  mdct_butterflies(init,w+n2,n2);
  mdct__bitreverse(init,w);

  /* roatate + window */

  T=init->trig+n2;
  x0=out+n2;

  for(i=0;i<n4;i++){
    x0--;
    out[i] =MULT_NORM((w[0]*T[0]+w[1]*T[1])*init->scale);
    x0[0]  =MULT_NORM((w[0]*T[1]-w[1]*T[0])*init->scale);
    w+=2;
    T+=2;
  }
}
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: *unnormalized* fft transform

 ********************************************************************/

/* FFT implementation from OggSquish, minus cosine transforms,
 * minus all but radix 2/4 case.  In Vorbis we only need this
 * cut-down version.
 *
 * To do more than just power-of-two sized vectors, see the full
 * version I wrote for NetLib.
 *
 * Note that the packing is a little strange; rather than the FFT r/i
 * packing following R_0, I_n, R_1, I_1, R_2, I_2 ... R_n-1, I_n-1,
 * it follows R_0, R_1, I_1, R_2, I_2 ... R_n-1, I_n-1, I_n like the
 * FORTRAN version
 */

#include <stdlib.h>
#include <string.h>
#include <math.h>
/*#include "smallft.h"*/
/*#include "os.h"*/
/*#include "misc.h"*/

static void drfti1(int n, float *wa, int *ifac){
  static int ntryh[4] = { 4,2,3,5 };
  static float tpi = 6.28318530717958648f;
  float arg,argh,argld,fi;
  int ntry=0,i,j=-1;
  int k1, l1, l2, ib;
  int ld, ii, ip, is, nq, nr;
  int ido, ipm, nfm1;
  int nl=n;
  int nf=0;

 L101:
  j++;
  if (j < 4)
    ntry=ntryh[j];
  else
    ntry+=2;

 L104:
  nq=nl/ntry;
  nr=nl-ntry*nq;
  if (nr!=0) goto L101;

  nf++;
  ifac[nf+1]=ntry;
  nl=nq;
  if(ntry!=2)goto L107;
  if(nf==1)goto L107;

  for (i=1;i<nf;i++){
    ib=nf-i+1;
    ifac[ib+1]=ifac[ib];
  }
  ifac[2] = 2;

 L107:
  if(nl!=1)goto L104;
  ifac[0]=n;
  ifac[1]=nf;
  argh=tpi/n;
  is=0;
  nfm1=nf-1;
  l1=1;

  if(nfm1==0)return;

  for (k1=0;k1<nfm1;k1++){
    ip=ifac[k1+2];
    ld=0;
    l2=l1*ip;
    ido=n/l2;
    ipm=ip-1;

    for (j=0;j<ipm;j++){
      ld+=l1;
      i=is;
      argld=(float)ld*argh;
      fi=0.f;
      for (ii=2;ii<ido;ii+=2){
        fi+=1.f;
        arg=fi*argld;
        wa[i++]=cos(arg);
        wa[i++]=sin(arg);
      }
      is+=ido;
    }
    l1=l2;
  }
}

static void fdrffti(int n, float *wsave, int *ifac){

  if (n == 1) return;
  drfti1(n, wsave+n, ifac);
}

static void dradf2(int ido,int l1,float *cc,float *ch,float *wa1){
  int i,k;
  float ti2,tr2;
  int t0,t1,t2,t3,t4,t5,t6;

  t1=0;
  t0=(t2=l1*ido);
  t3=ido<<1;
  for(k=0;k<l1;k++){
    ch[t1<<1]=cc[t1]+cc[t2];
    ch[(t1<<1)+t3-1]=cc[t1]-cc[t2];
    t1+=ido;
    t2+=ido;
  }

  if(ido<2)return;
  if(ido==2)goto L105;

  t1=0;
  t2=t0;
  for(k=0;k<l1;k++){
    t3=t2;
    t4=(t1<<1)+(ido<<1);
    t5=t1;
    t6=t1+t1;
    for(i=2;i<ido;i+=2){
      t3+=2;
      t4-=2;
      t5+=2;
      t6+=2;
      tr2=wa1[i-2]*cc[t3-1]+wa1[i-1]*cc[t3];
      ti2=wa1[i-2]*cc[t3]-wa1[i-1]*cc[t3-1];
      ch[t6]=cc[t5]+ti2;
      ch[t4]=ti2-cc[t5];
      ch[t6-1]=cc[t5-1]+tr2;
      ch[t4-1]=cc[t5-1]-tr2;
    }
    t1+=ido;
    t2+=ido;
  }

  if(ido%2==1)return;

 L105:
  t3=(t2=(t1=ido)-1);
  t2+=t0;
  for(k=0;k<l1;k++){
    ch[t1]=-cc[t2];
    ch[t1-1]=cc[t3];
    t1+=ido<<1;
    t2+=ido;
    t3+=ido;
  }
}

static void dradf4(int ido,int l1,float *cc,float *ch,float *wa1,
            float *wa2,float *wa3){
  static float hsqt2 = .70710678118654752f;
  int i,k,t0,t1,t2,t3,t4,t5,t6;
  float ci2,ci3,ci4,cr2,cr3,cr4,ti1,ti2,ti3,ti4,tr1,tr2,tr3,tr4;
  t0=l1*ido;

  t1=t0;
  t4=t1<<1;
  t2=t1+(t1<<1);
  t3=0;

  for(k=0;k<l1;k++){
    tr1=cc[t1]+cc[t2];
    tr2=cc[t3]+cc[t4];

    ch[t5=t3<<2]=tr1+tr2;
    ch[(ido<<2)+t5-1]=tr2-tr1;
    ch[(t5+=(ido<<1))-1]=cc[t3]-cc[t4];
    ch[t5]=cc[t2]-cc[t1];

    t1+=ido;
    t2+=ido;
    t3+=ido;
    t4+=ido;
  }

  if(ido<2)return;
  if(ido==2)goto L105;


  t1=0;
  for(k=0;k<l1;k++){
    t2=t1;
    t4=t1<<2;
    t5=(t6=ido<<1)+t4;
    for(i=2;i<ido;i+=2){
      t3=(t2+=2);
      t4+=2;
      t5-=2;

      t3+=t0;
      cr2=wa1[i-2]*cc[t3-1]+wa1[i-1]*cc[t3];
      ci2=wa1[i-2]*cc[t3]-wa1[i-1]*cc[t3-1];
      t3+=t0;
      cr3=wa2[i-2]*cc[t3-1]+wa2[i-1]*cc[t3];
      ci3=wa2[i-2]*cc[t3]-wa2[i-1]*cc[t3-1];
      t3+=t0;
      cr4=wa3[i-2]*cc[t3-1]+wa3[i-1]*cc[t3];
      ci4=wa3[i-2]*cc[t3]-wa3[i-1]*cc[t3-1];

      tr1=cr2+cr4;
      tr4=cr4-cr2;
      ti1=ci2+ci4;
      ti4=ci2-ci4;

      ti2=cc[t2]+ci3;
      ti3=cc[t2]-ci3;
      tr2=cc[t2-1]+cr3;
      tr3=cc[t2-1]-cr3;

      ch[t4-1]=tr1+tr2;
      ch[t4]=ti1+ti2;

      ch[t5-1]=tr3-ti4;
      ch[t5]=tr4-ti3;

      ch[t4+t6-1]=ti4+tr3;
      ch[t4+t6]=tr4+ti3;

      ch[t5+t6-1]=tr2-tr1;
      ch[t5+t6]=ti1-ti2;
    }
    t1+=ido;
  }
  if(ido&1)return;

 L105:

  t2=(t1=t0+ido-1)+(t0<<1);
  t3=ido<<2;
  t4=ido;
  t5=ido<<1;
  t6=ido;

  for(k=0;k<l1;k++){
    ti1=-hsqt2*(cc[t1]+cc[t2]);
    tr1=hsqt2*(cc[t1]-cc[t2]);

    ch[t4-1]=tr1+cc[t6-1];
    ch[t4+t5-1]=cc[t6-1]-tr1;

    ch[t4]=ti1-cc[t1+t0];
    ch[t4+t5]=ti1+cc[t1+t0];

    t1+=ido;
    t2+=ido;
    t4+=t3;
    t6+=ido;
  }
}

static void dradfg(int ido,int ip,int l1,int idl1,float *cc,float *c1,
                          float *c2,float *ch,float *ch2,float *wa){

  static float tpi=6.283185307179586f;
  int idij,ipph,i,j,k,l,ic,ik,is;
  int t0,t1,t2,t3,t4,t5,t6,t7,t8,t9,t10;
  float dc2,ai1,ai2,ar1,ar2,ds2;
  int nbd;
  float dcp,arg,dsp,ar1h,ar2h;
  int idp2,ipp2;

  arg=tpi/(float)ip;
  dcp=cos(arg);
  dsp=sin(arg);
  ipph=(ip+1)>>1;
  ipp2=ip;
  idp2=ido;
  nbd=(ido-1)>>1;
  t0=l1*ido;
  t10=ip*ido;

  if(ido==1)goto L119;
  for(ik=0;ik<idl1;ik++)ch2[ik]=c2[ik];

  t1=0;
  for(j=1;j<ip;j++){
    t1+=t0;
    t2=t1;
    for(k=0;k<l1;k++){
      ch[t2]=c1[t2];
      t2+=ido;
    }
  }

  is=-ido;
  t1=0;
  if(nbd>l1){
    for(j=1;j<ip;j++){
      t1+=t0;
      is+=ido;
      t2= -ido+t1;
      for(k=0;k<l1;k++){
        idij=is-1;
        t2+=ido;
        t3=t2;
        for(i=2;i<ido;i+=2){
          idij+=2;
          t3+=2;
          ch[t3-1]=wa[idij-1]*c1[t3-1]+wa[idij]*c1[t3];
          ch[t3]=wa[idij-1]*c1[t3]-wa[idij]*c1[t3-1];
        }
      }
    }
  }else{

    for(j=1;j<ip;j++){
      is+=ido;
      idij=is-1;
      t1+=t0;
      t2=t1;
      for(i=2;i<ido;i+=2){
        idij+=2;
        t2+=2;
        t3=t2;
        for(k=0;k<l1;k++){
          ch[t3-1]=wa[idij-1]*c1[t3-1]+wa[idij]*c1[t3];
          ch[t3]=wa[idij-1]*c1[t3]-wa[idij]*c1[t3-1];
          t3+=ido;
        }
      }
    }
  }

  t1=0;
  t2=ipp2*t0;
  if(nbd<l1){
    for(j=1;j<ipph;j++){
      t1+=t0;
      t2-=t0;
      t3=t1;
      t4=t2;
      for(i=2;i<ido;i+=2){
        t3+=2;
        t4+=2;
        t5=t3-ido;
        t6=t4-ido;
        for(k=0;k<l1;k++){
          t5+=ido;
          t6+=ido;
          c1[t5-1]=ch[t5-1]+ch[t6-1];
          c1[t6-1]=ch[t5]-ch[t6];
          c1[t5]=ch[t5]+ch[t6];
          c1[t6]=ch[t6-1]-ch[t5-1];
        }
      }
    }
  }else{
    for(j=1;j<ipph;j++){
      t1+=t0;
      t2-=t0;
      t3=t1;
      t4=t2;
      for(k=0;k<l1;k++){
        t5=t3;
        t6=t4;
        for(i=2;i<ido;i+=2){
          t5+=2;
          t6+=2;
          c1[t5-1]=ch[t5-1]+ch[t6-1];
          c1[t6-1]=ch[t5]-ch[t6];
          c1[t5]=ch[t5]+ch[t6];
          c1[t6]=ch[t6-1]-ch[t5-1];
        }
        t3+=ido;
        t4+=ido;
      }
    }
  }

L119:
  for(ik=0;ik<idl1;ik++)c2[ik]=ch2[ik];

  t1=0;
  t2=ipp2*idl1;
  for(j=1;j<ipph;j++){
    t1+=t0;
    t2-=t0;
    t3=t1-ido;
    t4=t2-ido;
    for(k=0;k<l1;k++){
      t3+=ido;
      t4+=ido;
      c1[t3]=ch[t3]+ch[t4];
      c1[t4]=ch[t4]-ch[t3];
    }
  }

  ar1=1.f;
  ai1=0.f;
  t1=0;
  t2=ipp2*idl1;
  t3=(ip-1)*idl1;
  for(l=1;l<ipph;l++){
    t1+=idl1;
    t2-=idl1;
    ar1h=dcp*ar1-dsp*ai1;
    ai1=dcp*ai1+dsp*ar1;
    ar1=ar1h;
    t4=t1;
    t5=t2;
    t6=t3;
    t7=idl1;

    for(ik=0;ik<idl1;ik++){
      ch2[t4++]=c2[ik]+ar1*c2[t7++];
      ch2[t5++]=ai1*c2[t6++];
    }

    dc2=ar1;
    ds2=ai1;
    ar2=ar1;
    ai2=ai1;

    t4=idl1;
    t5=(ipp2-1)*idl1;
    for(j=2;j<ipph;j++){
      t4+=idl1;
      t5-=idl1;

      ar2h=dc2*ar2-ds2*ai2;
      ai2=dc2*ai2+ds2*ar2;
      ar2=ar2h;

      t6=t1;
      t7=t2;
      t8=t4;
      t9=t5;
      for(ik=0;ik<idl1;ik++){
        ch2[t6++]+=ar2*c2[t8++];
        ch2[t7++]+=ai2*c2[t9++];
      }
    }
  }

  t1=0;
  for(j=1;j<ipph;j++){
    t1+=idl1;
    t2=t1;
    for(ik=0;ik<idl1;ik++)ch2[ik]+=c2[t2++];
  }

  if(ido<l1)goto L132;

  t1=0;
  t2=0;
  for(k=0;k<l1;k++){
    t3=t1;
    t4=t2;
    for(i=0;i<ido;i++)cc[t4++]=ch[t3++];
    t1+=ido;
    t2+=t10;
  }

  goto L135;

 L132:
  for(i=0;i<ido;i++){
    t1=i;
    t2=i;
    for(k=0;k<l1;k++){
      cc[t2]=ch[t1];
      t1+=ido;
      t2+=t10;
    }
  }

 L135:
  t1=0;
  t2=ido<<1;
  t3=0;
  t4=ipp2*t0;
  for(j=1;j<ipph;j++){

    t1+=t2;
    t3+=t0;
    t4-=t0;

    t5=t1;
    t6=t3;
    t7=t4;

    for(k=0;k<l1;k++){
      cc[t5-1]=ch[t6];
      cc[t5]=ch[t7];
      t5+=t10;
      t6+=ido;
      t7+=ido;
    }
  }

  if(ido==1)return;
  if(nbd<l1)goto L141;

  t1=-ido;
  t3=0;
  t4=0;
  t5=ipp2*t0;
  for(j=1;j<ipph;j++){
    t1+=t2;
    t3+=t2;
    t4+=t0;
    t5-=t0;
    t6=t1;
    t7=t3;
    t8=t4;
    t9=t5;
    for(k=0;k<l1;k++){
      for(i=2;i<ido;i+=2){
        ic=idp2-i;
        cc[i+t7-1]=ch[i+t8-1]+ch[i+t9-1];
        cc[ic+t6-1]=ch[i+t8-1]-ch[i+t9-1];
        cc[i+t7]=ch[i+t8]+ch[i+t9];
        cc[ic+t6]=ch[i+t9]-ch[i+t8];
      }
      t6+=t10;
      t7+=t10;
      t8+=ido;
      t9+=ido;
    }
  }
  return;

 L141:

  t1=-ido;
  t3=0;
  t4=0;
  t5=ipp2*t0;
  for(j=1;j<ipph;j++){
    t1+=t2;
    t3+=t2;
    t4+=t0;
    t5-=t0;
    for(i=2;i<ido;i+=2){
      t6=idp2+t1-i;
      t7=i+t3;
      t8=i+t4;
      t9=i+t5;
      for(k=0;k<l1;k++){
        cc[t7-1]=ch[t8-1]+ch[t9-1];
        cc[t6-1]=ch[t8-1]-ch[t9-1];
        cc[t7]=ch[t8]+ch[t9];
        cc[t6]=ch[t9]-ch[t8];
        t6+=t10;
        t7+=t10;
        t8+=ido;
        t9+=ido;
      }
    }
  }
}

static void drftf1(int n,float *c,float *ch,float *wa,int *ifac){
  int i,k1,l1,l2;
  int na,kh,nf;
  int ip,iw,ido,idl1,ix2,ix3;

  nf=ifac[1];
  na=1;
  l2=n;
  iw=n;

  for(k1=0;k1<nf;k1++){
    kh=nf-k1;
    ip=ifac[kh+1];
    l1=l2/ip;
    ido=n/l2;
    idl1=ido*l1;
    iw-=(ip-1)*ido;
    na=1-na;

    if(ip!=4)goto L102;

    ix2=iw+ido;
    ix3=ix2+ido;
    if(na!=0)
      dradf4(ido,l1,ch,c,wa+iw-1,wa+ix2-1,wa+ix3-1);
    else
      dradf4(ido,l1,c,ch,wa+iw-1,wa+ix2-1,wa+ix3-1);
    goto L110;

 L102:
    if(ip!=2)goto L104;
    if(na!=0)goto L103;

    dradf2(ido,l1,c,ch,wa+iw-1);
    goto L110;

  L103:
    dradf2(ido,l1,ch,c,wa+iw-1);
    goto L110;

  L104:
    if(ido==1)na=1-na;
    if(na!=0)goto L109;

    dradfg(ido,ip,l1,idl1,c,c,c,ch,ch,wa+iw-1);
    na=1;
    goto L110;

  L109:
    dradfg(ido,ip,l1,idl1,ch,ch,ch,c,c,wa+iw-1);
    na=0;

  L110:
    l2=l1;
  }

  if(na==1)return;

  for(i=0;i<n;i++)c[i]=ch[i];
}

static void dradb2(int ido,int l1,float *cc,float *ch,float *wa1){
  int i,k,t0,t1,t2,t3,t4,t5,t6;
  float ti2,tr2;

  t0=l1*ido;

  t1=0;
  t2=0;
  t3=(ido<<1)-1;
  for(k=0;k<l1;k++){
    ch[t1]=cc[t2]+cc[t3+t2];
    ch[t1+t0]=cc[t2]-cc[t3+t2];
    t2=(t1+=ido)<<1;
  }

  if(ido<2)return;
  if(ido==2)goto L105;

  t1=0;
  t2=0;
  for(k=0;k<l1;k++){
    t3=t1;
    t5=(t4=t2)+(ido<<1);
    t6=t0+t1;
    for(i=2;i<ido;i+=2){
      t3+=2;
      t4+=2;
      t5-=2;
      t6+=2;
      ch[t3-1]=cc[t4-1]+cc[t5-1];
      tr2=cc[t4-1]-cc[t5-1];
      ch[t3]=cc[t4]-cc[t5];
      ti2=cc[t4]+cc[t5];
      ch[t6-1]=wa1[i-2]*tr2-wa1[i-1]*ti2;
      ch[t6]=wa1[i-2]*ti2+wa1[i-1]*tr2;
    }
    t2=(t1+=ido)<<1;
  }

  if(ido%2==1)return;

L105:
  t1=ido-1;
  t2=ido-1;
  for(k=0;k<l1;k++){
    ch[t1]=cc[t2]+cc[t2];
    ch[t1+t0]=-(cc[t2+1]+cc[t2+1]);
    t1+=ido;
    t2+=ido<<1;
  }
}

static void dradb3(int ido,int l1,float *cc,float *ch,float *wa1,
                          float *wa2){
  static float taur = -.5f;
  static float taui = .8660254037844386f;
  int i,k,t0,t1,t2,t3,t4,t5,t6,t7,t8,t9,t10;
  float ci2,ci3,di2,di3,cr2,cr3,dr2,dr3,ti2,tr2;
  t0=l1*ido;

  t1=0;
  t2=t0<<1;
  t3=ido<<1;
  t4=ido+(ido<<1);
  t5=0;
  for(k=0;k<l1;k++){
    tr2=cc[t3-1]+cc[t3-1];
    cr2=cc[t5]+(taur*tr2);
    ch[t1]=cc[t5]+tr2;
    ci3=taui*(cc[t3]+cc[t3]);
    ch[t1+t0]=cr2-ci3;
    ch[t1+t2]=cr2+ci3;
    t1+=ido;
    t3+=t4;
    t5+=t4;
  }

  if(ido==1)return;

  t1=0;
  t3=ido<<1;
  for(k=0;k<l1;k++){
    t7=t1+(t1<<1);
    t6=(t5=t7+t3);
    t8=t1;
    t10=(t9=t1+t0)+t0;

    for(i=2;i<ido;i+=2){
      t5+=2;
      t6-=2;
      t7+=2;
      t8+=2;
      t9+=2;
      t10+=2;
      tr2=cc[t5-1]+cc[t6-1];
      cr2=cc[t7-1]+(taur*tr2);
      ch[t8-1]=cc[t7-1]+tr2;
      ti2=cc[t5]-cc[t6];
      ci2=cc[t7]+(taur*ti2);
      ch[t8]=cc[t7]+ti2;
      cr3=taui*(cc[t5-1]-cc[t6-1]);
      ci3=taui*(cc[t5]+cc[t6]);
      dr2=cr2-ci3;
      dr3=cr2+ci3;
      di2=ci2+cr3;
      di3=ci2-cr3;
      ch[t9-1]=wa1[i-2]*dr2-wa1[i-1]*di2;
      ch[t9]=wa1[i-2]*di2+wa1[i-1]*dr2;
      ch[t10-1]=wa2[i-2]*dr3-wa2[i-1]*di3;
      ch[t10]=wa2[i-2]*di3+wa2[i-1]*dr3;
    }
    t1+=ido;
  }
}

static void dradb4(int ido,int l1,float *cc,float *ch,float *wa1,
                          float *wa2,float *wa3){
  static float sqrt2=1.414213562373095f;
  int i,k,t0,t1,t2,t3,t4,t5,t6,t7,t8;
  float ci2,ci3,ci4,cr2,cr3,cr4,ti1,ti2,ti3,ti4,tr1,tr2,tr3,tr4;
  t0=l1*ido;

  t1=0;
  t2=ido<<2;
  t3=0;
  t6=ido<<1;
  for(k=0;k<l1;k++){
    t4=t3+t6;
    t5=t1;
    tr3=cc[t4-1]+cc[t4-1];
    tr4=cc[t4]+cc[t4];
    tr1=cc[t3]-cc[(t4+=t6)-1];
    tr2=cc[t3]+cc[t4-1];
    ch[t5]=tr2+tr3;
    ch[t5+=t0]=tr1-tr4;
    ch[t5+=t0]=tr2-tr3;
    ch[t5+=t0]=tr1+tr4;
    t1+=ido;
    t3+=t2;
  }

  if(ido<2)return;
  if(ido==2)goto L105;

  t1=0;
  for(k=0;k<l1;k++){
    t5=(t4=(t3=(t2=t1<<2)+t6))+t6;
    t7=t1;
    for(i=2;i<ido;i+=2){
      t2+=2;
      t3+=2;
      t4-=2;
      t5-=2;
      t7+=2;
      ti1=cc[t2]+cc[t5];
      ti2=cc[t2]-cc[t5];
      ti3=cc[t3]-cc[t4];
      tr4=cc[t3]+cc[t4];
      tr1=cc[t2-1]-cc[t5-1];
      tr2=cc[t2-1]+cc[t5-1];
      ti4=cc[t3-1]-cc[t4-1];
      tr3=cc[t3-1]+cc[t4-1];
      ch[t7-1]=tr2+tr3;
      cr3=tr2-tr3;
      ch[t7]=ti2+ti3;
      ci3=ti2-ti3;
      cr2=tr1-tr4;
      cr4=tr1+tr4;
      ci2=ti1+ti4;
      ci4=ti1-ti4;

      ch[(t8=t7+t0)-1]=wa1[i-2]*cr2-wa1[i-1]*ci2;
      ch[t8]=wa1[i-2]*ci2+wa1[i-1]*cr2;
      ch[(t8+=t0)-1]=wa2[i-2]*cr3-wa2[i-1]*ci3;
      ch[t8]=wa2[i-2]*ci3+wa2[i-1]*cr3;
      ch[(t8+=t0)-1]=wa3[i-2]*cr4-wa3[i-1]*ci4;
      ch[t8]=wa3[i-2]*ci4+wa3[i-1]*cr4;
    }
    t1+=ido;
  }

  if(ido%2 == 1)return;

 L105:

  t1=ido;
  t2=ido<<2;
  t3=ido-1;
  t4=ido+(ido<<1);
  for(k=0;k<l1;k++){
    t5=t3;
    ti1=cc[t1]+cc[t4];
    ti2=cc[t4]-cc[t1];
    tr1=cc[t1-1]-cc[t4-1];
    tr2=cc[t1-1]+cc[t4-1];
    ch[t5]=tr2+tr2;
    ch[t5+=t0]=sqrt2*(tr1-ti1);
    ch[t5+=t0]=ti2+ti2;
    ch[t5+=t0]=-sqrt2*(tr1+ti1);

    t3+=ido;
    t1+=t2;
    t4+=t2;
  }
}

static void dradbg(int ido,int ip,int l1,int idl1,float *cc,float *c1,
            float *c2,float *ch,float *ch2,float *wa){
  static float tpi=6.283185307179586f;
  int idij,ipph,i,j,k,l,ik,is,t0,t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,
      t11,t12;
  float dc2,ai1,ai2,ar1,ar2,ds2;
  int nbd;
  float dcp,arg,dsp,ar1h,ar2h;
  int ipp2;

  t10=ip*ido;
  t0=l1*ido;
  arg=tpi/(float)ip;
  dcp=cos(arg);
  dsp=sin(arg);
  nbd=(ido-1)>>1;
  ipp2=ip;
  ipph=(ip+1)>>1;
  if(ido<l1)goto L103;

  t1=0;
  t2=0;
  for(k=0;k<l1;k++){
    t3=t1;
    t4=t2;
    for(i=0;i<ido;i++){
      ch[t3]=cc[t4];
      t3++;
      t4++;
    }
    t1+=ido;
    t2+=t10;
  }
  goto L106;

 L103:
  t1=0;
  for(i=0;i<ido;i++){
    t2=t1;
    t3=t1;
    for(k=0;k<l1;k++){
      ch[t2]=cc[t3];
      t2+=ido;
      t3+=t10;
    }
    t1++;
  }

 L106:
  t1=0;
  t2=ipp2*t0;
  t7=(t5=ido<<1);
  for(j=1;j<ipph;j++){
    t1+=t0;
    t2-=t0;
    t3=t1;
    t4=t2;
    t6=t5;
    for(k=0;k<l1;k++){
      ch[t3]=cc[t6-1]+cc[t6-1];
      ch[t4]=cc[t6]+cc[t6];
      t3+=ido;
      t4+=ido;
      t6+=t10;
    }
    t5+=t7;
  }

  if (ido == 1)goto L116;
  if(nbd<l1)goto L112;

  t1=0;
  t2=ipp2*t0;
  t7=0;
  for(j=1;j<ipph;j++){
    t1+=t0;
    t2-=t0;
    t3=t1;
    t4=t2;

    t7+=(ido<<1);
    t8=t7;
    for(k=0;k<l1;k++){
      t5=t3;
      t6=t4;
      t9=t8;
      t11=t8;
      for(i=2;i<ido;i+=2){
        t5+=2;
        t6+=2;
        t9+=2;
        t11-=2;
        ch[t5-1]=cc[t9-1]+cc[t11-1];
        ch[t6-1]=cc[t9-1]-cc[t11-1];
        ch[t5]=cc[t9]-cc[t11];
        ch[t6]=cc[t9]+cc[t11];
      }
      t3+=ido;
      t4+=ido;
      t8+=t10;
    }
  }
  goto L116;

 L112:
  t1=0;
  t2=ipp2*t0;
  t7=0;
  for(j=1;j<ipph;j++){
    t1+=t0;
    t2-=t0;
    t3=t1;
    t4=t2;
    t7+=(ido<<1);
    t8=t7;
    t9=t7;
    for(i=2;i<ido;i+=2){
      t3+=2;
      t4+=2;
      t8+=2;
      t9-=2;
      t5=t3;
      t6=t4;
      t11=t8;
      t12=t9;
      for(k=0;k<l1;k++){
        ch[t5-1]=cc[t11-1]+cc[t12-1];
        ch[t6-1]=cc[t11-1]-cc[t12-1];
        ch[t5]=cc[t11]-cc[t12];
        ch[t6]=cc[t11]+cc[t12];
        t5+=ido;
        t6+=ido;
        t11+=t10;
        t12+=t10;
      }
    }
  }

L116:
  ar1=1.f;
  ai1=0.f;
  t1=0;
  t9=(t2=ipp2*idl1);
  t3=(ip-1)*idl1;
  for(l=1;l<ipph;l++){
    t1+=idl1;
    t2-=idl1;

    ar1h=dcp*ar1-dsp*ai1;
    ai1=dcp*ai1+dsp*ar1;
    ar1=ar1h;
    t4=t1;
    t5=t2;
    t6=0;
    t7=idl1;
    t8=t3;
    for(ik=0;ik<idl1;ik++){
      c2[t4++]=ch2[t6++]+ar1*ch2[t7++];
      c2[t5++]=ai1*ch2[t8++];
    }
    dc2=ar1;
    ds2=ai1;
    ar2=ar1;
    ai2=ai1;

    t6=idl1;
    t7=t9-idl1;
    for(j=2;j<ipph;j++){
      t6+=idl1;
      t7-=idl1;
      ar2h=dc2*ar2-ds2*ai2;
      ai2=dc2*ai2+ds2*ar2;
      ar2=ar2h;
      t4=t1;
      t5=t2;
      t11=t6;
      t12=t7;
      for(ik=0;ik<idl1;ik++){
        c2[t4++]+=ar2*ch2[t11++];
        c2[t5++]+=ai2*ch2[t12++];
      }
    }
  }

  t1=0;
  for(j=1;j<ipph;j++){
    t1+=idl1;
    t2=t1;
    for(ik=0;ik<idl1;ik++)ch2[ik]+=ch2[t2++];
  }

  t1=0;
  t2=ipp2*t0;
  for(j=1;j<ipph;j++){
    t1+=t0;
    t2-=t0;
    t3=t1;
    t4=t2;
    for(k=0;k<l1;k++){
      ch[t3]=c1[t3]-c1[t4];
      ch[t4]=c1[t3]+c1[t4];
      t3+=ido;
      t4+=ido;
    }
  }

  if(ido==1)goto L132;
  if(nbd<l1)goto L128;

  t1=0;
  t2=ipp2*t0;
  for(j=1;j<ipph;j++){
    t1+=t0;
    t2-=t0;
    t3=t1;
    t4=t2;
    for(k=0;k<l1;k++){
      t5=t3;
      t6=t4;
      for(i=2;i<ido;i+=2){
        t5+=2;
        t6+=2;
        ch[t5-1]=c1[t5-1]-c1[t6];
        ch[t6-1]=c1[t5-1]+c1[t6];
        ch[t5]=c1[t5]+c1[t6-1];
        ch[t6]=c1[t5]-c1[t6-1];
      }
      t3+=ido;
      t4+=ido;
    }
  }
  goto L132;

 L128:
  t1=0;
  t2=ipp2*t0;
  for(j=1;j<ipph;j++){
    t1+=t0;
    t2-=t0;
    t3=t1;
    t4=t2;
    for(i=2;i<ido;i+=2){
      t3+=2;
      t4+=2;
      t5=t3;
      t6=t4;
      for(k=0;k<l1;k++){
        ch[t5-1]=c1[t5-1]-c1[t6];
        ch[t6-1]=c1[t5-1]+c1[t6];
        ch[t5]=c1[t5]+c1[t6-1];
        ch[t6]=c1[t5]-c1[t6-1];
        t5+=ido;
        t6+=ido;
      }
    }
  }

L132:
  if(ido==1)return;

  for(ik=0;ik<idl1;ik++)c2[ik]=ch2[ik];

  t1=0;
  for(j=1;j<ip;j++){
    t2=(t1+=t0);
    for(k=0;k<l1;k++){
      c1[t2]=ch[t2];
      t2+=ido;
    }
  }

  if(nbd>l1)goto L139;

  is= -ido-1;
  t1=0;
  for(j=1;j<ip;j++){
    is+=ido;
    t1+=t0;
    idij=is;
    t2=t1;
    for(i=2;i<ido;i+=2){
      t2+=2;
      idij+=2;
      t3=t2;
      for(k=0;k<l1;k++){
        c1[t3-1]=wa[idij-1]*ch[t3-1]-wa[idij]*ch[t3];
        c1[t3]=wa[idij-1]*ch[t3]+wa[idij]*ch[t3-1];
        t3+=ido;
      }
    }
  }
  return;

 L139:
  is= -ido-1;
  t1=0;
  for(j=1;j<ip;j++){
    is+=ido;
    t1+=t0;
    t2=t1;
    for(k=0;k<l1;k++){
      idij=is;
      t3=t2;
      for(i=2;i<ido;i+=2){
        idij+=2;
        t3+=2;
        c1[t3-1]=wa[idij-1]*ch[t3-1]-wa[idij]*ch[t3];
        c1[t3]=wa[idij-1]*ch[t3]+wa[idij]*ch[t3-1];
      }
      t2+=ido;
    }
  }
}

static void drftb1(int n, float *c, float *ch, float *wa, int *ifac){
  int i,k1,l1,l2;
  int na;
  int nf,ip,iw,ix2,ix3,ido,idl1;

  nf=ifac[1];
  na=0;
  l1=1;
  iw=1;

  for(k1=0;k1<nf;k1++){
    ip=ifac[k1 + 2];
    l2=ip*l1;
    ido=n/l2;
    idl1=ido*l1;
    if(ip!=4)goto L103;
    ix2=iw+ido;
    ix3=ix2+ido;

    if(na!=0)
      dradb4(ido,l1,ch,c,wa+iw-1,wa+ix2-1,wa+ix3-1);
    else
      dradb4(ido,l1,c,ch,wa+iw-1,wa+ix2-1,wa+ix3-1);
    na=1-na;
    goto L115;

  L103:
    if(ip!=2)goto L106;

    if(na!=0)
      dradb2(ido,l1,ch,c,wa+iw-1);
    else
      dradb2(ido,l1,c,ch,wa+iw-1);
    na=1-na;
    goto L115;

  L106:
    if(ip!=3)goto L109;

    ix2=iw+ido;
    if(na!=0)
      dradb3(ido,l1,ch,c,wa+iw-1,wa+ix2-1);
    else
      dradb3(ido,l1,c,ch,wa+iw-1,wa+ix2-1);
    na=1-na;
    goto L115;

  L109:
/*    The radix five case can be translated later..... */
/*    if(ip!=5)goto L112;

    ix2=iw+ido;
    ix3=ix2+ido;
    ix4=ix3+ido;
    if(na!=0)
      dradb5(ido,l1,ch,c,wa+iw-1,wa+ix2-1,wa+ix3-1,wa+ix4-1);
    else
      dradb5(ido,l1,c,ch,wa+iw-1,wa+ix2-1,wa+ix3-1,wa+ix4-1);
    na=1-na;
    goto L115;

  L112:*/
    if(na!=0)
      dradbg(ido,ip,l1,idl1,ch,ch,ch,c,c,wa+iw-1);
    else
      dradbg(ido,ip,l1,idl1,c,c,c,ch,ch,wa+iw-1);
    if(ido==1)na=1-na;

  L115:
    l1=l2;
    iw+=(ip-1)*ido;
  }

  if(na==0)return;

  for(i=0;i<n;i++)c[i]=ch[i];
}

void drft_forward(drft_lookup *l,float *data){
  if(l->n==1)return;
  drftf1(l->n,data,l->trigcache,l->trigcache+l->n,l->splitcache);
}

void drft_backward(drft_lookup *l,float *data){
  if (l->n==1)return;
  drftb1(l->n,data,l->trigcache,l->trigcache+l->n,l->splitcache);
}

void drft_init(drft_lookup *l,int n){
  l->n=n;
  l->trigcache=(float*)_ogg_calloc(3*n,sizeof(*l->trigcache));
  l->splitcache=(int*)_ogg_calloc(32,sizeof(*l->splitcache));
  fdrffti(n, l->trigcache, l->splitcache);
}

void drft_clear(drft_lookup *l){
  if(l){
    if(l->trigcache)_ogg_free(l->trigcache);
    if(l->splitcache)_ogg_free(l->splitcache);
    memset(l,0,sizeof(*l));
  }
}
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2015             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: PCM data vector blocking, windowing and dis/reassembly

 Handle windowing, overlap-add, etc of the PCM vectors.  This is made
 more amusing by Vorbis' current two allowed block sizes.

 ********************************************************************/

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
/*#include <ogg/ogg.h>*/
/*#include "vorbis/codec.h"*/
/*#include "codec_internal.h"*/

/*#include "window.h"*/
/*#include "mdct.h"*/
/*#include "lpc.h"*/
/*#include "registry.h"*/
/*#include "misc.h"*/

/* pcm accumulator examples (not exhaustive):

 <-------------- lW ---------------->
                   <--------------- W ---------------->
:            .....|.....       _______________         |
:        .'''     |     '''_---      |       |\        |
:.....'''         |_____--- '''......|       | \_______|
:.................|__________________|_______|__|______|
                  |<------ Sl ------>|      > Sr <     |endW
                  |beginSl           |endSl  |  |endSr
                  |beginW            |endlW  |beginSr


                      |< lW >|
                   <--------------- W ---------------->
                  |   |  ..  ______________            |
                  |   | '  `/        |     ---_        |
                  |___.'___/`.       |         ---_____|
                  |_______|__|_______|_________________|
                  |      >|Sl|<      |<------ Sr ----->|endW
                  |       |  |endSl  |beginSr          |endSr
                  |beginW |  |endlW
                  mult[0] |beginSl                     mult[n]

 <-------------- lW ----------------->
                          |<--W-->|
:            ..............  ___  |   |
:        .'''             |`/   \ |   |
:.....'''                 |/`....\|...|
:.........................|___|___|___|
                          |Sl |Sr |endW
                          |   |   |endSr
                          |   |beginSr
                          |   |endSl
                          |beginSl
                          |beginW
*/

/* block abstraction setup *********************************************/

#ifndef WORD_ALIGN
#define WORD_ALIGN 8
#endif

int vorbis_block_init(vorbis_dsp_state *v, vorbis_block *vb){
  int i;
  memset(vb,0,sizeof(*vb));
  vb->vd=v;
  vb->localalloc=0;
  vb->localstore=NULL;
  if(v->analysisp){
    vorbis_block_internal* vbi;
    vb->internal=vbi=(vorbis_block_internal*)_ogg_calloc(1,sizeof(vorbis_block_internal));
    vbi->ampmax=-9999;

    for(i=0;i<PACKETBLOBS;i++){
      if(i==PACKETBLOBS/2){
        vbi->packetblob[i]=&vb->opb;
      }else{
        vbi->packetblob[i]=
          (oggpack_buffer*)_ogg_calloc(1,sizeof(oggpack_buffer));
      }
      oggpack_writeinit(vbi->packetblob[i]);
    }
  }

  return(0);
}

void *_vorbis_block_alloc(vorbis_block *vb,long bytes){
  bytes=(bytes+(WORD_ALIGN-1)) & ~(WORD_ALIGN-1);
  if(bytes+vb->localtop>vb->localalloc){
    /* can't just _ogg_realloc... there are outstanding pointers */
    if(vb->localstore){
      struct alloc_chain *link=(alloc_chain*)_ogg_malloc(sizeof(*link));
      vb->totaluse+=vb->localtop;
      link->next=vb->reap;
      link->ptr=vb->localstore;
      vb->reap=link;
    }
    /* highly conservative */
    vb->localalloc=bytes;
    vb->localstore=_ogg_malloc(vb->localalloc);
    vb->localtop=0;
  }
  {
    void *ret=(void *)(((char *)vb->localstore)+vb->localtop);
    vb->localtop+=bytes;
    return ret;
  }
}

/* reap the chain, pull the ripcord */
void _vorbis_block_ripcord(vorbis_block *vb){
  /* reap the chain */
  struct alloc_chain *reap=vb->reap;
  while(reap){
    struct alloc_chain *next=reap->next;
    _ogg_free(reap->ptr);
    memset(reap,0,sizeof(*reap));
    _ogg_free(reap);
    reap=next;
  }
  /* consolidate storage */
  if(vb->totaluse){
    vb->localstore=_ogg_realloc(vb->localstore,vb->totaluse+vb->localalloc);
    vb->localalloc+=vb->totaluse;
    vb->totaluse=0;
  }

  /* pull the ripcord */
  vb->localtop=0;
  vb->reap=NULL;
}

int vorbis_block_clear(vorbis_block *vb){
  int i;
  vorbis_block_internal *vbi=(vorbis_block_internal*)vb->internal;

  _vorbis_block_ripcord(vb);
  if(vb->localstore)_ogg_free(vb->localstore);

  if(vbi){
    for(i=0;i<PACKETBLOBS;i++){
      oggpack_writeclear(vbi->packetblob[i]);
      if(i!=PACKETBLOBS/2)_ogg_free(vbi->packetblob[i]);
    }
    _ogg_free(vbi);
  }
  memset(vb,0,sizeof(*vb));
  return(0);
}

/* Analysis side code, but directly related to blocking.  Thus it's
   here and not in analysis.c (which is for analysis transforms only).
   The init is here because some of it is shared */

static int _vds_shared_init(vorbis_dsp_state *v,vorbis_info *vi,int encp){
  int i;
  codec_setup_info *ci=(codec_setup_info*)vi->codec_setup;
  private_state *b=NULL;
  int hs;

  if(ci==NULL||
     ci->modes<=0||
     ci->blocksizes[0]<64||
     ci->blocksizes[1]<ci->blocksizes[0]){
    return 1;
  }
  hs=ci->halfrate_flag;

  memset(v,0,sizeof(*v));
  v->backend_state=b=(private_state*)_ogg_calloc(1,sizeof(*b));

  v->vi=vi;
  b->modebits=ov_ilog(ci->modes-1);

  b->transform[0]=(vorbis_look_transform**)_ogg_calloc(VI_TRANSFORMB,sizeof(*b->transform[0]));
  b->transform[1]=(vorbis_look_transform**)_ogg_calloc(VI_TRANSFORMB,sizeof(*b->transform[1]));

  /* MDCT is tranform 0 */

  b->transform[0][0]=_ogg_calloc(1,sizeof(mdct_lookup));
  b->transform[1][0]=_ogg_calloc(1,sizeof(mdct_lookup));
  mdct_init((mdct_lookup*)b->transform[0][0],ci->blocksizes[0]>>hs);
  mdct_init((mdct_lookup*)b->transform[1][0],ci->blocksizes[1]>>hs);

  /* Vorbis I uses only window type 0 */
  /* note that the correct computation below is technically:
       b->window[0]=ov_ilog(ci->blocksizes[0]-1)-6;
       b->window[1]=ov_ilog(ci->blocksizes[1]-1)-6;
    but since blocksizes are always powers of two,
    the below is equivalent.
   */
  b->window[0]=ov_ilog(ci->blocksizes[0])-7;
  b->window[1]=ov_ilog(ci->blocksizes[1])-7;

  if(encp){ /* encode/decode differ here */

    /* analysis always needs an fft */
    drft_init(&b->fft_look[0],ci->blocksizes[0]);
    drft_init(&b->fft_look[1],ci->blocksizes[1]);

    /* finish the codebooks */
    if(!ci->fullbooks){
      ci->fullbooks= (codebook*)_ogg_calloc(ci->books,sizeof(*ci->fullbooks));
      for(i=0;i<ci->books;i++)
        vorbis_book_init_encode(ci->fullbooks+i,ci->book_param[i]);
    }

    b->psy=(vorbis_look_psy*)_ogg_calloc(ci->psys,sizeof(*b->psy));
    for(i=0;i<ci->psys;i++){
      _vp_psy_init(b->psy+i,
                   ci->psy_param[i],
                   &ci->psy_g_param,
                   ci->blocksizes[ci->psy_param[i]->blockflag]/2,
                   vi->rate);
    }

    v->analysisp=1;
  }else{
    /* finish the codebooks */
    if(!ci->fullbooks){
      ci->fullbooks=(codebook*)_ogg_calloc(ci->books,sizeof(*ci->fullbooks));
      for(i=0;i<ci->books;i++){
        if(ci->book_param[i]==NULL)
          goto abort_books;
        if(vorbis_book_init_decode(ci->fullbooks+i,ci->book_param[i]))
          goto abort_books;
        /* decode codebooks are now standalone after init */
        vorbis_staticbook_destroy(ci->book_param[i]);
        ci->book_param[i]=NULL;
      }
    }
  }

  /* initialize the storage vectors. blocksize[1] is small for encode,
     but the correct size for decode */
  v->pcm_storage=ci->blocksizes[1];
  v->pcm=(float**)_ogg_malloc(vi->channels*sizeof(*v->pcm));
  v->pcmret=(float**)_ogg_malloc(vi->channels*sizeof(*v->pcmret));
  {
    int i;
    for(i=0;i<vi->channels;i++)
      v->pcm[i]=(float*)_ogg_calloc(v->pcm_storage,sizeof(*v->pcm[i]));
  }

  /* all 1 (large block) or 0 (small block) */
  /* explicitly set for the sake of clarity */
  v->lW=0; /* previous window size */
  v->W=0;  /* current window size */

  /* all vector indexes */
  v->centerW=ci->blocksizes[1]/2;

  v->pcm_current=v->centerW;

  /* initialize all the backend lookups */
  b->flr=(vorbis_look_floor**)_ogg_calloc(ci->floors,sizeof(*b->flr));
  b->residue=(vorbis_look_residue**)_ogg_calloc(ci->residues,sizeof(*b->residue));

  for(i=0;i<ci->floors;i++)
    b->flr[i]=_floor_P[ci->floor_type[i]]->
      look(v,ci->floor_param[i]);

  for(i=0;i<ci->residues;i++)
    b->residue[i]=_residue_P[ci->residue_type[i]]->
      look(v,ci->residue_param[i]);

  return 0;
 abort_books:
  for(i=0;i<ci->books;i++){
    if(ci->book_param[i]!=NULL){
      vorbis_staticbook_destroy(ci->book_param[i]);
      ci->book_param[i]=NULL;
    }
  }
  vorbis_dsp_clear(v);
  return -1;
}

/* arbitrary settings and spec-mandated numbers get filled in here */
int vorbis_analysis_init(vorbis_dsp_state *v,vorbis_info *vi){
  private_state *b=NULL;

  if(_vds_shared_init(v,vi,1))return 1;
  b=(private_state*)v->backend_state;
  b->psy_g_look=_vp_global_look(vi);

  /* Initialize the envelope state storage */
  b->ve=(envelope_lookup*)_ogg_calloc(1,sizeof(*b->ve));
  _ve_envelope_init(b->ve,vi);

  vorbis_bitrate_init(vi,&b->bms);

  /* compressed audio packets start after the headers
     with sequence number 3 */
  v->sequence=3;

  return(0);
}

void vorbis_dsp_clear(vorbis_dsp_state *v){
  int i;
  if(v){
    vorbis_info *vi=v->vi;
    codec_setup_info *ci=(codec_setup_info*)(vi?vi->codec_setup:NULL);
    private_state *b=(private_state*)v->backend_state;

    if(b){

      if(b->ve){
        _ve_envelope_clear(b->ve);
        _ogg_free(b->ve);
      }

      if(b->transform[0]){
        mdct_clear((mdct_lookup*)b->transform[0][0]);
        _ogg_free(b->transform[0][0]);
        _ogg_free(b->transform[0]);
      }
      if(b->transform[1]){
        mdct_clear((mdct_lookup*)b->transform[1][0]);
        _ogg_free(b->transform[1][0]);
        _ogg_free(b->transform[1]);
      }

      if(b->flr){
        if(ci)
          for(i=0;i<ci->floors;i++)
            _floor_P[ci->floor_type[i]]->
              free_look(b->flr[i]);
        _ogg_free(b->flr);
      }
      if(b->residue){
        if(ci)
          for(i=0;i<ci->residues;i++)
            _residue_P[ci->residue_type[i]]->
              free_look(b->residue[i]);
        _ogg_free(b->residue);
      }
      if(b->psy){
        if(ci)
          for(i=0;i<ci->psys;i++)
            _vp_psy_clear(b->psy+i);
        _ogg_free(b->psy);
      }

      if(b->psy_g_look)_vp_global_free(b->psy_g_look);
      vorbis_bitrate_clear(&b->bms);

      drft_clear(&b->fft_look[0]);
      drft_clear(&b->fft_look[1]);

    }

    if(v->pcm){
      if(vi)
        for(i=0;i<vi->channels;i++)
          if(v->pcm[i])_ogg_free(v->pcm[i]);
      _ogg_free(v->pcm);
      if(v->pcmret)_ogg_free(v->pcmret);
    }

    if(b){
      /* free header, header1, header2 */
      if(b->header)_ogg_free(b->header);
      if(b->header1)_ogg_free(b->header1);
      if(b->header2)_ogg_free(b->header2);
      _ogg_free(b);
    }

    memset(v,0,sizeof(*v));
  }
}

float **vorbis_analysis_buffer(vorbis_dsp_state *v, int vals){
  int i;
  vorbis_info *vi=v->vi;
  private_state *b=(private_state*)v->backend_state;

  /* free header, header1, header2 */
  if(b->header)_ogg_free(b->header);b->header=NULL;
  if(b->header1)_ogg_free(b->header1);b->header1=NULL;
  if(b->header2)_ogg_free(b->header2);b->header2=NULL;

  /* Do we have enough storage space for the requested buffer? If not,
     expand the PCM (and envelope) storage */

  if(v->pcm_current+vals>=v->pcm_storage){
    v->pcm_storage=v->pcm_current+vals*2;

    for(i=0;i<vi->channels;i++){
      v->pcm[i]=(float*)_ogg_realloc(v->pcm[i],v->pcm_storage*sizeof(*v->pcm[i]));
    }
  }

  for(i=0;i<vi->channels;i++)
    v->pcmret[i]=v->pcm[i]+v->pcm_current;

  return(v->pcmret);
}

static void _preextrapolate_helper(vorbis_dsp_state *v){
  int i;
  int order=16;
  float *lpc=(float*)alloca(order*sizeof(*lpc));
  float *work= (float*)alloca(v->pcm_current*sizeof(*work));
  long j;
  v->preextrapolate=1;

  if(v->pcm_current-v->centerW>order*2){ /* safety */
    for(i=0;i<v->vi->channels;i++){
      /* need to run the extrapolation in reverse! */
      for(j=0;j<v->pcm_current;j++)
        work[j]=v->pcm[i][v->pcm_current-j-1];

      /* prime as above */
      vorbis_lpc_from_data(work,lpc,v->pcm_current-v->centerW,order);

#if 0
      if(v->vi->channels==2){
        if(i==0)
          _analysis_output("predataL",0,work,v->pcm_current-v->centerW,0,0,0);
        else
          _analysis_output("predataR",0,work,v->pcm_current-v->centerW,0,0,0);
      }else{
        _analysis_output("predata",0,work,v->pcm_current-v->centerW,0,0,0);
      }
#endif

      /* run the predictor filter */
      vorbis_lpc_predict(lpc,work+v->pcm_current-v->centerW-order,
                         order,
                         work+v->pcm_current-v->centerW,
                         v->centerW);

      for(j=0;j<v->pcm_current;j++)
        v->pcm[i][v->pcm_current-j-1]=work[j];

    }
  }
}


/* call with val<=0 to set eof */

int vorbis_analysis_wrote(vorbis_dsp_state *v, int vals){
  vorbis_info *vi=v->vi;
  codec_setup_info *ci=(codec_setup_info*)vi->codec_setup;

  if(vals<=0){
    int order=32;
    int i;
    float *lpc=(float*)alloca(order*sizeof(*lpc));

    /* if it wasn't done earlier (very short sample) */
    if(!v->preextrapolate)
      _preextrapolate_helper(v);

    /* We're encoding the end of the stream.  Just make sure we have
       [at least] a few full blocks of zeroes at the end. */
    /* actually, we don't want zeroes; that could drop a large
       amplitude off a cliff, creating spread spectrum noise that will
       suck to encode.  Extrapolate for the sake of cleanliness. */

    vorbis_analysis_buffer(v,ci->blocksizes[1]*3);
    v->eofflag=v->pcm_current;
    v->pcm_current+=ci->blocksizes[1]*3;

    for(i=0;i<vi->channels;i++){
      if(v->eofflag>order*2){
        /* extrapolate with LPC to fill in */
        long n;

        /* make a predictor filter */
        n=v->eofflag;
        if(n>ci->blocksizes[1])n=ci->blocksizes[1];
        vorbis_lpc_from_data(v->pcm[i]+v->eofflag-n,lpc,n,order);

        /* run the predictor filter */
        vorbis_lpc_predict(lpc,v->pcm[i]+v->eofflag-order,order,
                           v->pcm[i]+v->eofflag,v->pcm_current-v->eofflag);
      }else{
        /* not enough data to extrapolate (unlikely to happen due to
           guarding the overlap, but bulletproof in case that
           assumtion goes away). zeroes will do. */
        memset(v->pcm[i]+v->eofflag,0,
               (v->pcm_current-v->eofflag)*sizeof(*v->pcm[i]));

      }
    }
  }else{

    if(v->pcm_current+vals>v->pcm_storage)
      return(OV_EINVAL);

    v->pcm_current+=vals;

    /* we may want to reverse extrapolate the beginning of a stream
       too... in case we're beginning on a cliff! */
    /* clumsy, but simple.  It only runs once, so simple is good. */
    if(!v->preextrapolate && v->pcm_current-v->centerW>ci->blocksizes[1])
      _preextrapolate_helper(v);

  }
  return(0);
}

/* do the deltas, envelope shaping, pre-echo and determine the size of
   the next block on which to continue analysis */
int vorbis_analysis_blockout(vorbis_dsp_state *v,vorbis_block *vb){
  int i;
  vorbis_info *vi=v->vi;
  codec_setup_info *ci=(codec_setup_info*)vi->codec_setup;
  private_state *b=(private_state*)v->backend_state;
  vorbis_look_psy_global *g=b->psy_g_look;
  long beginW=v->centerW-ci->blocksizes[v->W]/2,centerNext;
  vorbis_block_internal *vbi=(vorbis_block_internal *)vb->internal;

  /* check to see if we're started... */
  if(!v->preextrapolate)return(0);

  /* check to see if we're done... */
  if(v->eofflag==-1)return(0);

  /* By our invariant, we have lW, W and centerW set.  Search for
     the next boundary so we can determine nW (the next window size)
     which lets us compute the shape of the current block's window */

  /* we do an envelope search even on a single blocksize; we may still
     be throwing more bits at impulses, and envelope search handles
     marking impulses too. */
  {
    long bp=_ve_envelope_search(v);
    if(bp==-1){

      if(v->eofflag==0)return(0); /* not enough data currently to search for a
                                     full long block */
      v->nW=0;
    }else{

      if(ci->blocksizes[0]==ci->blocksizes[1])
        v->nW=0;
      else
        v->nW=bp;
    }
  }

  centerNext=v->centerW+ci->blocksizes[v->W]/4+ci->blocksizes[v->nW]/4;

  {
    /* center of next block + next block maximum right side. */

    long blockbound=centerNext+ci->blocksizes[v->nW]/2;
    if(v->pcm_current<blockbound)return(0); /* not enough data yet;
                                               although this check is
                                               less strict that the
                                               _ve_envelope_search,
                                               the search is not run
                                               if we only use one
                                               block size */


  }

  /* fill in the block.  Note that for a short window, lW and nW are *short*
     regardless of actual settings in the stream */

  _vorbis_block_ripcord(vb);
  vb->lW=v->lW;
  vb->W=v->W;
  vb->nW=v->nW;

  if(v->W){
    if(!v->lW || !v->nW){
      vbi->blocktype=BLOCKTYPE_TRANSITION;
      /*fprintf(stderr,"-");*/
    }else{
      vbi->blocktype=BLOCKTYPE_LONG;
      /*fprintf(stderr,"_");*/
    }
  }else{
    if(_ve_envelope_mark(v)){
      vbi->blocktype=BLOCKTYPE_IMPULSE;
      /*fprintf(stderr,"|");*/

    }else{
      vbi->blocktype=BLOCKTYPE_PADDING;
      /*fprintf(stderr,".");*/

    }
  }

  vb->vd=v;
  vb->sequence=v->sequence++;
  vb->granulepos=v->granulepos;
  vb->pcmend=ci->blocksizes[v->W];

  /* copy the vectors; this uses the local storage in vb */

  /* this tracks 'strongest peak' for later psychoacoustics */
  /* moved to the global psy state; clean this mess up */
  if(vbi->ampmax>g->ampmax)g->ampmax=vbi->ampmax;
  g->ampmax=_vp_ampmax_decay(g->ampmax,v);
  vbi->ampmax=g->ampmax;

  vb->pcm=(float**)_vorbis_block_alloc(vb,sizeof(*vb->pcm)*vi->channels);
  vbi->pcmdelay= (float**)_vorbis_block_alloc(vb,sizeof(*vbi->pcmdelay)*vi->channels);
  for(i=0;i<vi->channels;i++){
    vbi->pcmdelay[i]=(float*)
      _vorbis_block_alloc(vb,(vb->pcmend+beginW)*sizeof(*vbi->pcmdelay[i]));
    memcpy(vbi->pcmdelay[i],v->pcm[i],(vb->pcmend+beginW)*sizeof(*vbi->pcmdelay[i]));
    vb->pcm[i]=vbi->pcmdelay[i]+beginW;

    /* before we added the delay
       vb->pcm[i]=_vorbis_block_alloc(vb,vb->pcmend*sizeof(*vb->pcm[i]));
       memcpy(vb->pcm[i],v->pcm[i]+beginW,ci->blocksizes[v->W]*sizeof(*vb->pcm[i]));
    */

  }

  /* handle eof detection: eof==0 means that we've not yet received EOF
                           eof>0  marks the last 'real' sample in pcm[]
                           eof<0  'no more to do'; doesn't get here */

  if(v->eofflag){
    if(v->centerW>=v->eofflag){
      v->eofflag=-1;
      vb->eofflag=1;
      return(1);
    }
  }

  /* advance storage vectors and clean up */
  {
    int new_centerNext=ci->blocksizes[1]/2;
    int movementW=centerNext-new_centerNext;

    if(movementW>0){

      _ve_envelope_shift(b->ve,movementW);
      v->pcm_current-=movementW;

      for(i=0;i<vi->channels;i++)
        memmove(v->pcm[i],v->pcm[i]+movementW,
                v->pcm_current*sizeof(*v->pcm[i]));


      v->lW=v->W;
      v->W=v->nW;
      v->centerW=new_centerNext;

      if(v->eofflag){
        v->eofflag-=movementW;
        if(v->eofflag<=0)v->eofflag=-1;
        /* do not add padding to end of stream! */
        if(v->centerW>=v->eofflag){
          v->granulepos+=movementW-(v->centerW-v->eofflag);
        }else{
          v->granulepos+=movementW;
        }
      }else{
        v->granulepos+=movementW;
      }
    }
  }

  /* done */
  return(1);
}

int vorbis_synthesis_restart(vorbis_dsp_state *v){
  vorbis_info *vi=v->vi;
  codec_setup_info *ci;
  int hs;

  if(!v->backend_state)return -1;
  if(!vi)return -1;
  ci=(codec_setup_info*)vi->codec_setup;
  if(!ci)return -1;
  hs=ci->halfrate_flag;

  v->centerW=ci->blocksizes[1]>>(hs+1);
  v->pcm_current=v->centerW>>hs;

  v->pcm_returned=-1;
  v->granulepos=-1;
  v->sequence=-1;
  v->eofflag=0;
  ((private_state *)(v->backend_state))->sample_count=-1;

  return(0);
}

int vorbis_synthesis_init(vorbis_dsp_state *v,vorbis_info *vi){
  if(_vds_shared_init(v,vi,0)){
    vorbis_dsp_clear(v);
    return 1;
  }
  vorbis_synthesis_restart(v);
  return 0;
}

/* Unlike in analysis, the window is only partially applied for each
   block.  The time domain envelope is not yet handled at the point of
   calling (as it relies on the previous block). */

int vorbis_synthesis_blockin(vorbis_dsp_state *v,vorbis_block *vb){
  vorbis_info *vi=v->vi;
  codec_setup_info *ci=(codec_setup_info*)vi->codec_setup;
  private_state *b=(private_state*)v->backend_state;
  int hs=ci->halfrate_flag;
  int i,j;

  if(!vb)return(OV_EINVAL);
  if(v->pcm_current>v->pcm_returned  && v->pcm_returned!=-1)return(OV_EINVAL);

  v->lW=v->W;
  v->W=vb->W;
  v->nW=-1;

  if((v->sequence==-1)||
     (v->sequence+1 != vb->sequence)){
    v->granulepos=-1; /* out of sequence; lose count */
    b->sample_count=-1;
  }

  v->sequence=vb->sequence;

  if(vb->pcm){  /* no pcm to process if vorbis_synthesis_trackonly
                   was called on block */
    int n=ci->blocksizes[v->W]>>(hs+1);
    int n0=ci->blocksizes[0]>>(hs+1);
    int n1=ci->blocksizes[1]>>(hs+1);

    int thisCenter;
    int prevCenter;

    v->glue_bits+=vb->glue_bits;
    v->time_bits+=vb->time_bits;
    v->floor_bits+=vb->floor_bits;
    v->res_bits+=vb->res_bits;

    if(v->centerW){
      thisCenter=n1;
      prevCenter=0;
    }else{
      thisCenter=0;
      prevCenter=n1;
    }

    /* v->pcm is now used like a two-stage double buffer.  We don't want
       to have to constantly shift *or* adjust memory usage.  Don't
       accept a new block until the old is shifted out */

    for(j=0;j<vi->channels;j++){
      /* the overlap/add section */
      if(v->lW){
        if(v->W){
          /* large/large */
          const float *w=_vorbis_window_get(b->window[1]-hs);
          float *pcm=v->pcm[j]+prevCenter;
          float *p=vb->pcm[j];
          for(i=0;i<n1;i++)
            pcm[i]=pcm[i]*w[n1-i-1] + p[i]*w[i];
        }else{
          /* large/small */
          const float *w=_vorbis_window_get(b->window[0]-hs);
          float *pcm=v->pcm[j]+prevCenter+n1/2-n0/2;
          float *p=vb->pcm[j];
          for(i=0;i<n0;i++)
            pcm[i]=pcm[i]*w[n0-i-1] +p[i]*w[i];
        }
      }else{
        if(v->W){
          /* small/large */
          const float *w=_vorbis_window_get(b->window[0]-hs);
          float *pcm=v->pcm[j]+prevCenter;
          float *p=vb->pcm[j]+n1/2-n0/2;
          for(i=0;i<n0;i++)
            pcm[i]=pcm[i]*w[n0-i-1] +p[i]*w[i];
          for(;i<n1/2+n0/2;i++)
            pcm[i]=p[i];
        }else{
          /* small/small */
          const float *w=_vorbis_window_get(b->window[0]-hs);
          float *pcm=v->pcm[j]+prevCenter;
          float *p=vb->pcm[j];
          for(i=0;i<n0;i++)
            pcm[i]=pcm[i]*w[n0-i-1] +p[i]*w[i];
        }
      }

      /* the copy section */
      {
        float *pcm=v->pcm[j]+thisCenter;
        float *p=vb->pcm[j]+n;
        for(i=0;i<n;i++)
          pcm[i]=p[i];
      }
    }

    if(v->centerW)
      v->centerW=0;
    else
      v->centerW=n1;

    /* deal with initial packet state; we do this using the explicit
       pcm_returned==-1 flag otherwise we're sensitive to first block
       being short or long */

    if(v->pcm_returned==-1){
      v->pcm_returned=thisCenter;
      v->pcm_current=thisCenter;
    }else{
      v->pcm_returned=prevCenter;
      v->pcm_current=prevCenter+
        ((ci->blocksizes[v->lW]/4+
        ci->blocksizes[v->W]/4)>>hs);
    }

  }

  /* track the frame number... This is for convenience, but also
     making sure our last packet doesn't end with added padding.  If
     the last packet is partial, the number of samples we'll have to
     return will be past the vb->granulepos.

     This is not foolproof!  It will be confused if we begin
     decoding at the last page after a seek or hole.  In that case,
     we don't have a starting point to judge where the last frame
     is.  For this reason, vorbisfile will always try to make sure
     it reads the last two marked pages in proper sequence */

  if(b->sample_count==-1){
    b->sample_count=0;
  }else{
    b->sample_count+=ci->blocksizes[v->lW]/4+ci->blocksizes[v->W]/4;
  }

  if(v->granulepos==-1){
    if(vb->granulepos!=-1){ /* only set if we have a position to set to */

      v->granulepos=vb->granulepos;

      /* is this a short page? */
      if(b->sample_count>v->granulepos){
        /* corner case; if this is both the first and last audio page,
           then spec says the end is cut, not beginning */
       long extra=b->sample_count-vb->granulepos;

        /* we use ogg_int64_t for granule positions because a
           uint64 isn't universally available.  Unfortunately,
           that means granposes can be 'negative' and result in
           extra being negative */
        if(extra<0)
          extra=0;

        if(vb->eofflag){
          /* trim the end */
          /* no preceding granulepos; assume we started at zero (we'd
             have to in a short single-page stream) */
          /* granulepos could be -1 due to a seek, but that would result
             in a long count, not short count */

          /* Guard against corrupt/malicious frames that set EOP and
             a backdated granpos; don't rewind more samples than we
             actually have */
          if(extra > (v->pcm_current - v->pcm_returned)<<hs)
            extra = (v->pcm_current - v->pcm_returned)<<hs;

          v->pcm_current-=extra>>hs;
        }else{
          /* trim the beginning */
          v->pcm_returned+=extra>>hs;
          if(v->pcm_returned>v->pcm_current)
            v->pcm_returned=v->pcm_current;
        }

      }

    }
  }else{
    v->granulepos+=ci->blocksizes[v->lW]/4+ci->blocksizes[v->W]/4;
    if(vb->granulepos!=-1 && v->granulepos!=vb->granulepos){

      if(v->granulepos>vb->granulepos){
        long extra=v->granulepos-vb->granulepos;

        if(extra)
          if(vb->eofflag){
            /* partial last frame.  Strip the extra samples off */

            /* Guard against corrupt/malicious frames that set EOP and
               a backdated granpos; don't rewind more samples than we
               actually have */
            if(extra > (v->pcm_current - v->pcm_returned)<<hs)
              extra = (v->pcm_current - v->pcm_returned)<<hs;

            /* we use ogg_int64_t for granule positions because a
               uint64 isn't universally available.  Unfortunately,
               that means granposes can be 'negative' and result in
               extra being negative */
            if(extra<0)
              extra=0;

            v->pcm_current-=extra>>hs;
          } /* else {Shouldn't happen *unless* the bitstream is out of
               spec.  Either way, believe the bitstream } */
      } /* else {Shouldn't happen *unless* the bitstream is out of
           spec.  Either way, believe the bitstream } */
      v->granulepos=vb->granulepos;
    }
  }

  /* Update, cleanup */

  if(vb->eofflag)v->eofflag=1;
  return(0);

}

/* pcm==NULL indicates we just want the pending samples, no more */
int vorbis_synthesis_pcmout(vorbis_dsp_state *v,float ***pcm){
  vorbis_info *vi=v->vi;

  if(v->pcm_returned>-1 && v->pcm_returned<v->pcm_current){
    if(pcm){
      int i;
      for(i=0;i<vi->channels;i++)
        v->pcmret[i]=v->pcm[i]+v->pcm_returned;
      *pcm=v->pcmret;
    }
    return(v->pcm_current-v->pcm_returned);
  }
  return(0);
}

int vorbis_synthesis_read(vorbis_dsp_state *v,int n){
  if(n && v->pcm_returned+n>v->pcm_current)return(OV_EINVAL);
  v->pcm_returned+=n;
  return(0);
}

/* intended for use with a specific vorbisfile feature; we want access
   to the [usually synthetic/postextrapolated] buffer and lapping at
   the end of a decode cycle, specifically, a half-short-block worth.
   This funtion works like pcmout above, except it will also expose
   this implicit buffer data not normally decoded. */
int vorbis_synthesis_lapout(vorbis_dsp_state *v,float ***pcm){
  vorbis_info *vi=v->vi;
  codec_setup_info *ci=(codec_setup_info*)vi->codec_setup;
  int hs=ci->halfrate_flag;

  int n=ci->blocksizes[v->W]>>(hs+1);
  int n0=ci->blocksizes[0]>>(hs+1);
  int n1=ci->blocksizes[1]>>(hs+1);
  int i,j;

  if(v->pcm_returned<0)return 0;

  /* our returned data ends at pcm_returned; because the synthesis pcm
     buffer is a two-fragment ring, that means our data block may be
     fragmented by buffering, wrapping or a short block not filling
     out a buffer.  To simplify things, we unfragment if it's at all
     possibly needed. Otherwise, we'd need to call lapout more than
     once as well as hold additional dsp state.  Opt for
     simplicity. */

  /* centerW was advanced by blockin; it would be the center of the
     *next* block */
  if(v->centerW==n1){
    /* the data buffer wraps; swap the halves */
    /* slow, sure, small */
    for(j=0;j<vi->channels;j++){
      float *p=v->pcm[j];
      for(i=0;i<n1;i++){
        float temp=p[i];
        p[i]=p[i+n1];
        p[i+n1]=temp;
      }
    }

    v->pcm_current-=n1;
    v->pcm_returned-=n1;
    v->centerW=0;
  }

  /* solidify buffer into contiguous space */
  if((v->lW^v->W)==1){
    /* long/short or short/long */
    for(j=0;j<vi->channels;j++){
      float *s=v->pcm[j];
      float *d=v->pcm[j]+(n1-n0)/2;
      for(i=(n1+n0)/2-1;i>=0;--i)
        d[i]=s[i];
    }
    v->pcm_returned+=(n1-n0)/2;
    v->pcm_current+=(n1-n0)/2;
  }else{
    if(v->lW==0){
      /* short/short */
      for(j=0;j<vi->channels;j++){
        float *s=v->pcm[j];
        float *d=v->pcm[j]+n1-n0;
        for(i=n0-1;i>=0;--i)
          d[i]=s[i];
      }
      v->pcm_returned+=n1-n0;
      v->pcm_current+=n1-n0;
    }
  }

  if(pcm){
    int i;
    for(i=0;i<vi->channels;i++)
      v->pcmret[i]=v->pcm[i]+v->pcm_returned;
    *pcm=v->pcmret;
  }

  return(n1+n-v->pcm_returned);

}

const float *vorbis_window(vorbis_dsp_state *v,int W){
  vorbis_info *vi=v->vi;
  codec_setup_info *ci=(codec_setup_info*)vi->codec_setup;
  int hs=ci->halfrate_flag;
  private_state *b= (private_state*)v->backend_state;

  if(b->window[W]-1<0)return NULL;
  return _vorbis_window_get(b->window[W]-hs);
}
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: PCM data envelope analysis

 ********************************************************************/

#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include <math.h>
/*#include <ogg/ogg.h>*/
/*#include "vorbis/codec.h"*/
/*#include "codec_internal.h"*/

/*#include "os.h"*/
/*#include "scales.h"*/
/*#include "envelope.h"*/
/*#include "mdct.h"*/
/*#include "misc.h"*/

void _ve_envelope_init(envelope_lookup *e,vorbis_info *vi){
  codec_setup_info *ci= (codec_setup_info*)vi->codec_setup;
  vorbis_info_psy_global *gi=&ci->psy_g_param;
  int ch=vi->channels;
  int i,j;
  int n=e->winlength=128;
  e->searchstep=64; /* not random */

  e->minenergy=gi->preecho_minenergy;
  e->ch=ch;
  e->storage=128;
  e->cursor=ci->blocksizes[1]/2;
  e->mdct_win=(float*)_ogg_calloc(n,sizeof(*e->mdct_win));
  mdct_init(&e->mdct,n);

  for(i=0;i<n;i++){
    e->mdct_win[i]=sin(i/(n-1.)*M_PI);
    e->mdct_win[i]*=e->mdct_win[i];
  }

  /* magic follows */
  e->band[0].begin=2;  e->band[0].end=4;
  e->band[1].begin=4;  e->band[1].end=5;
  e->band[2].begin=6;  e->band[2].end=6;
  e->band[3].begin=9;  e->band[3].end=8;
  e->band[4].begin=13;  e->band[4].end=8;
  e->band[5].begin=17;  e->band[5].end=8;
  e->band[6].begin=22;  e->band[6].end=8;

  for(j=0;j<VE_BANDS;j++){
    n=e->band[j].end;
    e->band[j].window=(float*)_ogg_malloc(n*sizeof(*e->band[0].window));
    for(i=0;i<n;i++){
      e->band[j].window[i]=sin((i+.5)/n*M_PI);
      e->band[j].total+=e->band[j].window[i];
    }
    e->band[j].total=1./e->band[j].total;
  }

  e->filter=(envelope_filter_state*)_ogg_calloc(VE_BANDS*ch,sizeof(*e->filter));
  e->mark=(int*)_ogg_calloc(e->storage,sizeof(*e->mark));

}

void _ve_envelope_clear(envelope_lookup *e){
  int i;
  mdct_clear(&e->mdct);
  for(i=0;i<VE_BANDS;i++)
    _ogg_free(e->band[i].window);
  _ogg_free(e->mdct_win);
  _ogg_free(e->filter);
  _ogg_free(e->mark);
  memset(e,0,sizeof(*e));
}

/* fairly straight threshhold-by-band based until we find something
   that works better and isn't patented. */

static int _ve_amp(envelope_lookup *ve,
                   vorbis_info_psy_global *gi,
                   float *data,
                   envelope_band *bands,
                   envelope_filter_state *filters){
  long n=ve->winlength;
  int ret=0;
  long i,j;
  float decay;

  /* we want to have a 'minimum bar' for energy, else we're just
     basing blocks on quantization noise that outweighs the signal
     itself (for low power signals) */

  float minV=ve->minenergy;
  float *vec=(float*)alloca(n*sizeof(*vec));

  /* stretch is used to gradually lengthen the number of windows
     considered prevoius-to-potential-trigger */
  int stretch=max(VE_MINSTRETCH,ve->stretch/2);
  float penalty=gi->stretch_penalty-(ve->stretch/2-VE_MINSTRETCH);
  if(penalty<0.f)penalty=0.f;
  if(penalty>gi->stretch_penalty)penalty=gi->stretch_penalty;

  /*_analysis_output_always("lpcm",seq2,data,n,0,0,
    totalshift+pos*ve->searchstep);*/

 /* window and transform */
  for(i=0;i<n;i++)
    vec[i]=data[i]*ve->mdct_win[i];
  mdct_forward(&ve->mdct,vec,vec);

  /*_analysis_output_always("mdct",seq2,vec,n/2,0,1,0); */

  /* near-DC spreading function; this has nothing to do with
     psychoacoustics, just sidelobe leakage and window size */
  {
    float temp=vec[0]*vec[0]+.7*vec[1]*vec[1]+.2*vec[2]*vec[2];
    int ptr=filters->nearptr;

    /* the accumulation is regularly refreshed from scratch to avoid
       floating point creep */
    if(ptr==0){
      decay=filters->nearDC_acc=filters->nearDC_partialacc+temp;
      filters->nearDC_partialacc=temp;
    }else{
      decay=filters->nearDC_acc+=temp;
      filters->nearDC_partialacc+=temp;
    }
    filters->nearDC_acc-=filters->nearDC[ptr];
    filters->nearDC[ptr]=temp;

    decay*=(1./(VE_NEARDC+1));
    filters->nearptr++;
    if(filters->nearptr>=VE_NEARDC)filters->nearptr=0;
    decay=todB(&decay)*.5-15.f;
  }

  /* perform spreading and limiting, also smooth the spectrum.  yes,
     the MDCT results in all real coefficients, but it still *behaves*
     like real/imaginary pairs */
  for(i=0;i<n/2;i+=2){
    float val=vec[i]*vec[i]+vec[i+1]*vec[i+1];
    val=todB(&val)*.5f;
    if(val<decay)val=decay;
    if(val<minV)val=minV;
    vec[i>>1]=val;
    decay-=8.;
  }

  /*_analysis_output_always("spread",seq2++,vec,n/4,0,0,0);*/

  /* perform preecho/postecho triggering by band */
  for(j=0;j<VE_BANDS;j++){
    float acc=0.;
    float valmax,valmin;

    /* accumulate amplitude */
    for(i=0;i<bands[j].end;i++)
      acc+=vec[i+bands[j].begin]*bands[j].window[i];

    acc*=bands[j].total;

    /* convert amplitude to delta */
    {
      int p, this_;
      p = this_ =filters[j].ampptr;
      float postmax,postmin,premax=-99999.f,premin=99999.f;

      p=this_;
      p--;
      if(p<0)p+=VE_AMP;
      postmax=max(acc,filters[j].ampbuf[p]);
      postmin=min(acc,filters[j].ampbuf[p]);

      for(i=0;i<stretch;i++){
        p--;
        if(p<0)p+=VE_AMP;
        premax=max(premax,filters[j].ampbuf[p]);
        premin=min(premin,filters[j].ampbuf[p]);
      }

      valmin=postmin-premin;
      valmax=postmax-premax;

      /*filters[j].markers[pos]=valmax;*/
      filters[j].ampbuf[this_]=acc;
      filters[j].ampptr++;
      if(filters[j].ampptr>=VE_AMP)filters[j].ampptr=0;
    }

    /* look at min/max, decide trigger */
    if(valmax>gi->preecho_thresh[j]+penalty){
      ret|=1;
      ret|=4;
    }
    if(valmin<gi->postecho_thresh[j]-penalty)ret|=2;
  }

  return(ret);
}

#if 0
static int seq=0;
static ogg_int64_t totalshift=-1024;
#endif

long _ve_envelope_search(vorbis_dsp_state *v){
  vorbis_info *vi=v->vi;
  codec_setup_info *ci=(codec_setup_info*)vi->codec_setup;
  vorbis_info_psy_global *gi=&ci->psy_g_param;
  envelope_lookup *ve=((private_state *)(v->backend_state))->ve;
  long i,j;

  int first=ve->current/ve->searchstep;
  int last=v->pcm_current/ve->searchstep-VE_WIN;
  if(first<0)first=0;

  /* make sure we have enough storage to match the PCM */
  if(last+VE_WIN+VE_POST>ve->storage){
    ve->storage=last+VE_WIN+VE_POST; /* be sure */
    ve->mark=(int*)_ogg_realloc(ve->mark,ve->storage*sizeof(*ve->mark));
  }

  for(j=first;j<last;j++){
    int ret=0;

    ve->stretch++;
    if(ve->stretch>VE_MAXSTRETCH*2)
      ve->stretch=VE_MAXSTRETCH*2;

    for(i=0;i<ve->ch;i++){
      float *pcm=v->pcm[i]+ve->searchstep*(j);
      ret|=_ve_amp(ve,gi,pcm,ve->band,ve->filter+i*VE_BANDS);
    }

    ve->mark[j+VE_POST]=0;
    if(ret&1){
      ve->mark[j]=1;
      ve->mark[j+1]=1;
    }

    if(ret&2){
      ve->mark[j]=1;
      if(j>0)ve->mark[j-1]=1;
    }

    if(ret&4)ve->stretch=-1;
  }

  ve->current=last*ve->searchstep;

  {
    long centerW=v->centerW;
    long testW=
      centerW+
      ci->blocksizes[v->W]/4+
      ci->blocksizes[1]/2+
      ci->blocksizes[0]/4;

    j=ve->cursor;

    while(j<ve->current-(ve->searchstep)){/* account for postecho
                                             working back one window */
      if(j>=testW)return(1);

      ve->cursor=j;

      if(ve->mark[j/ve->searchstep]){
        if(j>centerW){

#if 0
          if(j>ve->curmark){
            float *marker=alloca(v->pcm_current*sizeof(*marker));
            int l,m;
            memset(marker,0,sizeof(*marker)*v->pcm_current);
            fprintf(stderr,"mark! seq=%d, cursor:%fs time:%fs\n",
                    seq,
                    (totalshift+ve->cursor)/44100.,
                    (totalshift+j)/44100.);
            _analysis_output_always("pcmL",seq,v->pcm[0],v->pcm_current,0,0,totalshift);
            _analysis_output_always("pcmR",seq,v->pcm[1],v->pcm_current,0,0,totalshift);

            _analysis_output_always("markL",seq,v->pcm[0],j,0,0,totalshift);
            _analysis_output_always("markR",seq,v->pcm[1],j,0,0,totalshift);

            for(m=0;m<VE_BANDS;m++){
              char buf[80];
              sprintf(buf,"delL%d",m);
              for(l=0;l<last;l++)marker[l*ve->searchstep]=ve->filter[m].markers[l]*.1;
              _analysis_output_always(buf,seq,marker,v->pcm_current,0,0,totalshift);
            }

            for(m=0;m<VE_BANDS;m++){
              char buf[80];
              sprintf(buf,"delR%d",m);
              for(l=0;l<last;l++)marker[l*ve->searchstep]=ve->filter[m+VE_BANDS].markers[l]*.1;
              _analysis_output_always(buf,seq,marker,v->pcm_current,0,0,totalshift);
            }

            for(l=0;l<last;l++)marker[l*ve->searchstep]=ve->mark[l]*.4;
            _analysis_output_always("mark",seq,marker,v->pcm_current,0,0,totalshift);


            seq++;

          }
#endif

          ve->curmark=j;
          if(j>=testW)return(1);
          return(0);
        }
      }
      j+=ve->searchstep;
    }
  }

  return(-1);
}

int _ve_envelope_mark(vorbis_dsp_state *v){
  envelope_lookup *ve=((private_state *)(v->backend_state))->ve;
  vorbis_info *vi=v->vi;
  codec_setup_info *ci=(codec_setup_info*)vi->codec_setup;
  long centerW=v->centerW;
  long beginW=centerW-ci->blocksizes[v->W]/4;
  long endW=centerW+ci->blocksizes[v->W]/4;
  if(v->W){
    beginW-=ci->blocksizes[v->lW]/4;
    endW+=ci->blocksizes[v->nW]/4;
  }else{
    beginW-=ci->blocksizes[0]/4;
    endW+=ci->blocksizes[0]/4;
  }

  if(ve->curmark>=beginW && ve->curmark<endW)return(1);
  {
    long first=beginW/ve->searchstep;
    long last=endW/ve->searchstep;
    long i;
    for(i=first;i<last;i++)
      if(ve->mark[i])return(1);
  }
  return(0);
}

void _ve_envelope_shift(envelope_lookup *e,long shift){
  int smallsize=e->current/e->searchstep+VE_POST; /* adjust for placing marks
                                                     ahead of ve->current */
  int smallshift=shift/e->searchstep;

  memmove(e->mark,e->mark+smallshift,(smallsize-smallshift)*sizeof(*e->mark));

#if 0
  for(i=0;i<VE_BANDS*e->ch;i++)
    memmove(e->filter[i].markers,
            e->filter[i].markers+smallshift,
            (1024-smallshift)*sizeof(*(*e->filter).markers));
  totalshift+=shift;
#endif

  e->current-=shift;
  if(e->curmark>=0)
    e->curmark-=shift;
  e->cursor-=shift;
}
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: window functions

 ********************************************************************/

#include <stdlib.h>
#include <math.h>
/*#include "os.h"*/
/*#include "misc.h"*/
/*#include "window.h"*/

static const float vwin64[32] = {
  0.0009460463F, 0.0085006468F, 0.0235352254F, 0.0458950567F,
  0.0753351908F, 0.1115073077F, 0.1539457973F, 0.2020557475F,
  0.2551056759F, 0.3122276645F, 0.3724270287F, 0.4346027792F,
  0.4975789974F, 0.5601459521F, 0.6211085051F, 0.6793382689F,
  0.7338252629F, 0.7837245849F, 0.8283939355F, 0.8674186656F,
  0.9006222429F, 0.9280614787F, 0.9500073081F, 0.9669131782F,
  0.9793740220F, 0.9880792941F, 0.9937636139F, 0.9971582668F,
  0.9989462667F, 0.9997230082F, 0.9999638688F, 0.9999995525F,
};

static const float vwin128[64] = {
  0.0002365472F, 0.0021280687F, 0.0059065254F, 0.0115626550F,
  0.0190823442F, 0.0284463735F, 0.0396300935F, 0.0526030430F,
  0.0673285281F, 0.0837631763F, 0.1018564887F, 0.1215504095F,
  0.1427789367F, 0.1654677960F, 0.1895342001F, 0.2148867160F,
  0.2414252576F, 0.2690412240F, 0.2976177952F, 0.3270303960F,
  0.3571473350F, 0.3878306189F, 0.4189369387F, 0.4503188188F,
  0.4818259135F, 0.5133064334F, 0.5446086751F, 0.5755826278F,
  0.6060816248F, 0.6359640047F, 0.6650947483F, 0.6933470543F,
  0.7206038179F, 0.7467589810F, 0.7717187213F, 0.7954024542F,
  0.8177436264F, 0.8386902831F, 0.8582053981F, 0.8762669622F,
  0.8928678298F, 0.9080153310F, 0.9217306608F, 0.9340480615F,
  0.9450138200F, 0.9546851041F, 0.9631286621F, 0.9704194171F,
  0.9766389810F, 0.9818741197F, 0.9862151938F, 0.9897546035F,
  0.9925852598F, 0.9947991032F, 0.9964856900F, 0.9977308602F,
  0.9986155015F, 0.9992144193F, 0.9995953200F, 0.9998179155F,
  0.9999331503F, 0.9999825563F, 0.9999977357F, 0.9999999720F,
};

static const float vwin256[128] = {
  0.0000591390F, 0.0005321979F, 0.0014780301F, 0.0028960636F,
  0.0047854363F, 0.0071449926F, 0.0099732775F, 0.0132685298F,
  0.0170286741F, 0.0212513119F, 0.0259337111F, 0.0310727950F,
  0.0366651302F, 0.0427069140F, 0.0491939614F, 0.0561216907F,
  0.0634851102F, 0.0712788035F, 0.0794969160F, 0.0881331402F,
  0.0971807028F, 0.1066323515F, 0.1164803426F, 0.1267164297F,
  0.1373318534F, 0.1483173323F, 0.1596630553F, 0.1713586755F,
  0.1833933062F, 0.1957555184F, 0.2084333404F, 0.2214142599F,
  0.2346852280F, 0.2482326664F, 0.2620424757F, 0.2761000481F,
  0.2903902813F, 0.3048975959F, 0.3196059553F, 0.3344988887F,
  0.3495595160F, 0.3647705766F, 0.3801144597F, 0.3955732382F,
  0.4111287047F, 0.4267624093F, 0.4424557009F, 0.4581897696F,
  0.4739456913F, 0.4897044744F, 0.5054471075F, 0.5211546088F,
  0.5368080763F, 0.5523887395F, 0.5678780103F, 0.5832575361F,
  0.5985092508F, 0.6136154277F, 0.6285587300F, 0.6433222619F,
  0.6578896175F, 0.6722449294F, 0.6863729144F, 0.7002589187F,
  0.7138889597F, 0.7272497662F, 0.7403288154F, 0.7531143679F,
  0.7655954985F, 0.7777621249F, 0.7896050322F, 0.8011158947F,
  0.8122872932F, 0.8231127294F, 0.8335866365F, 0.8437043850F,
  0.8534622861F, 0.8628575905F, 0.8718884835F, 0.8805540765F,
  0.8888543947F, 0.8967903616F, 0.9043637797F, 0.9115773078F,
  0.9184344360F, 0.9249394562F, 0.9310974312F, 0.9369141608F,
  0.9423961446F, 0.9475505439F, 0.9523851406F, 0.9569082947F,
  0.9611289005F, 0.9650563408F, 0.9687004405F, 0.9720714191F,
  0.9751798427F, 0.9780365753F, 0.9806527301F, 0.9830396204F,
  0.9852087111F, 0.9871715701F, 0.9889398207F, 0.9905250941F,
  0.9919389832F, 0.9931929973F, 0.9942985174F, 0.9952667537F,
  0.9961087037F, 0.9968351119F, 0.9974564312F, 0.9979827858F,
  0.9984239359F, 0.9987892441F, 0.9990876435F, 0.9993276081F,
  0.9995171241F, 0.9996636648F, 0.9997741654F, 0.9998550016F,
  0.9999119692F, 0.9999502656F, 0.9999744742F, 0.9999885497F,
  0.9999958064F, 0.9999989077F, 0.9999998584F, 0.9999999983F,
};

static const float vwin512[256] = {
  0.0000147849F, 0.0001330607F, 0.0003695946F, 0.0007243509F,
  0.0011972759F, 0.0017882983F, 0.0024973285F, 0.0033242588F,
  0.0042689632F, 0.0053312973F, 0.0065110982F, 0.0078081841F,
  0.0092223540F, 0.0107533880F, 0.0124010466F, 0.0141650703F,
  0.0160451800F, 0.0180410758F, 0.0201524373F, 0.0223789233F,
  0.0247201710F, 0.0271757958F, 0.0297453914F, 0.0324285286F,
  0.0352247556F, 0.0381335972F, 0.0411545545F, 0.0442871045F,
  0.0475306997F, 0.0508847676F, 0.0543487103F, 0.0579219038F,
  0.0616036982F, 0.0653934164F, 0.0692903546F, 0.0732937809F,
  0.0774029356F, 0.0816170305F, 0.0859352485F, 0.0903567428F,
  0.0948806375F, 0.0995060259F, 0.1042319712F, 0.1090575056F,
  0.1139816300F, 0.1190033137F, 0.1241214941F, 0.1293350764F,
  0.1346429333F, 0.1400439046F, 0.1455367974F, 0.1511203852F,
  0.1567934083F, 0.1625545735F, 0.1684025537F, 0.1743359881F,
  0.1803534820F, 0.1864536069F, 0.1926349000F, 0.1988958650F,
  0.2052349715F, 0.2116506555F, 0.2181413191F, 0.2247053313F,
  0.2313410275F, 0.2380467105F, 0.2448206500F, 0.2516610835F,
  0.2585662164F, 0.2655342226F, 0.2725632448F, 0.2796513950F,
  0.2867967551F, 0.2939973773F, 0.3012512852F, 0.3085564739F,
  0.3159109111F, 0.3233125375F, 0.3307592680F, 0.3382489922F,
  0.3457795756F, 0.3533488602F, 0.3609546657F, 0.3685947904F,
  0.3762670121F, 0.3839690896F, 0.3916987634F, 0.3994537572F,
  0.4072317788F, 0.4150305215F, 0.4228476653F, 0.4306808783F,
  0.4385278181F, 0.4463861329F, 0.4542534630F, 0.4621274424F,
  0.4700057001F, 0.4778858615F, 0.4857655502F, 0.4936423891F,
  0.5015140023F, 0.5093780165F, 0.5172320626F, 0.5250737772F,
  0.5329008043F, 0.5407107971F, 0.5485014192F, 0.5562703465F,
  0.5640152688F, 0.5717338914F, 0.5794239366F, 0.5870831457F,
  0.5947092801F, 0.6023001235F, 0.6098534829F, 0.6173671907F,
  0.6248391059F, 0.6322671161F, 0.6396491384F, 0.6469831217F,
  0.6542670475F, 0.6614989319F, 0.6686768267F, 0.6757988210F,
  0.6828630426F, 0.6898676592F, 0.6968108799F, 0.7036909564F,
  0.7105061843F, 0.7172549043F, 0.7239355032F, 0.7305464154F,
  0.7370861235F, 0.7435531598F, 0.7499461068F, 0.7562635986F,
  0.7625043214F, 0.7686670148F, 0.7747504721F, 0.7807535410F,
  0.7866751247F, 0.7925141825F, 0.7982697296F, 0.8039408387F,
  0.8095266395F, 0.8150263196F, 0.8204391248F, 0.8257643590F,
  0.8310013848F, 0.8361496236F, 0.8412085555F, 0.8461777194F,
  0.8510567129F, 0.8558451924F, 0.8605428730F, 0.8651495278F,
  0.8696649882F, 0.8740891432F, 0.8784219392F, 0.8826633797F,
  0.8868135244F, 0.8908724888F, 0.8948404441F, 0.8987176157F,
  0.9025042831F, 0.9062007791F, 0.9098074886F, 0.9133248482F,
  0.9167533451F, 0.9200935163F, 0.9233459472F, 0.9265112712F,
  0.9295901680F, 0.9325833632F, 0.9354916263F, 0.9383157705F,
  0.9410566504F, 0.9437151618F, 0.9462922398F, 0.9487888576F,
  0.9512060252F, 0.9535447882F, 0.9558062262F, 0.9579914516F,
  0.9601016078F, 0.9621378683F, 0.9641014348F, 0.9659935361F,
  0.9678154261F, 0.9695683830F, 0.9712537071F, 0.9728727198F,
  0.9744267618F, 0.9759171916F, 0.9773453842F, 0.9787127293F,
  0.9800206298F, 0.9812705006F, 0.9824637665F, 0.9836018613F,
  0.9846862258F, 0.9857183066F, 0.9866995544F, 0.9876314227F,
  0.9885153662F, 0.9893528393F, 0.9901452948F, 0.9908941823F,
  0.9916009470F, 0.9922670279F, 0.9928938570F, 0.9934828574F,
  0.9940354423F, 0.9945530133F, 0.9950369595F, 0.9954886562F,
  0.9959094633F, 0.9963007242F, 0.9966637649F, 0.9969998925F,
  0.9973103939F, 0.9975965351F, 0.9978595598F, 0.9981006885F,
  0.9983211172F, 0.9985220166F, 0.9987045311F, 0.9988697776F,
  0.9990188449F, 0.9991527924F, 0.9992726499F, 0.9993794157F,
  0.9994740570F, 0.9995575079F, 0.9996306699F, 0.9996944099F,
  0.9997495605F, 0.9997969190F, 0.9998372465F, 0.9998712678F,
  0.9998996704F, 0.9999231041F, 0.9999421807F, 0.9999574732F,
  0.9999695157F, 0.9999788026F, 0.9999857885F, 0.9999908879F,
  0.9999944746F, 0.9999968817F, 0.9999984010F, 0.9999992833F,
  0.9999997377F, 0.9999999317F, 0.9999999911F, 0.9999999999F,
};

static const float vwin1024[512] = {
  0.0000036962F, 0.0000332659F, 0.0000924041F, 0.0001811086F,
  0.0002993761F, 0.0004472021F, 0.0006245811F, 0.0008315063F,
  0.0010679699F, 0.0013339631F, 0.0016294757F, 0.0019544965F,
  0.0023090133F, 0.0026930125F, 0.0031064797F, 0.0035493989F,
  0.0040217533F, 0.0045235250F, 0.0050546946F, 0.0056152418F,
  0.0062051451F, 0.0068243817F, 0.0074729278F, 0.0081507582F,
  0.0088578466F, 0.0095941655F, 0.0103596863F, 0.0111543789F,
  0.0119782122F, 0.0128311538F, 0.0137131701F, 0.0146242260F,
  0.0155642855F, 0.0165333111F, 0.0175312640F, 0.0185581042F,
  0.0196137903F, 0.0206982797F, 0.0218115284F, 0.0229534910F,
  0.0241241208F, 0.0253233698F, 0.0265511886F, 0.0278075263F,
  0.0290923308F, 0.0304055484F, 0.0317471241F, 0.0331170013F,
  0.0345151222F, 0.0359414274F, 0.0373958560F, 0.0388783456F,
  0.0403888325F, 0.0419272511F, 0.0434935347F, 0.0450876148F,
  0.0467094213F, 0.0483588828F, 0.0500359261F, 0.0517404765F,
  0.0534724575F, 0.0552317913F, 0.0570183983F, 0.0588321971F,
  0.0606731048F, 0.0625410369F, 0.0644359070F, 0.0663576272F,
  0.0683061077F, 0.0702812571F, 0.0722829821F, 0.0743111878F,
  0.0763657775F, 0.0784466526F, 0.0805537129F, 0.0826868561F,
  0.0848459782F, 0.0870309736F, 0.0892417345F, 0.0914781514F,
  0.0937401128F, 0.0960275056F, 0.0983402145F, 0.1006781223F,
  0.1030411101F, 0.1054290568F, 0.1078418397F, 0.1102793336F,
  0.1127414119F, 0.1152279457F, 0.1177388042F, 0.1202738544F,
  0.1228329618F, 0.1254159892F, 0.1280227980F, 0.1306532471F,
  0.1333071937F, 0.1359844927F, 0.1386849970F, 0.1414085575F,
  0.1441550230F, 0.1469242403F, 0.1497160539F, 0.1525303063F,
  0.1553668381F, 0.1582254875F, 0.1611060909F, 0.1640084822F,
  0.1669324936F, 0.1698779549F, 0.1728446939F, 0.1758325362F,
  0.1788413055F, 0.1818708232F, 0.1849209084F, 0.1879913785F,
  0.1910820485F, 0.1941927312F, 0.1973232376F, 0.2004733764F,
  0.2036429541F, 0.2068317752F, 0.2100396421F, 0.2132663552F,
  0.2165117125F, 0.2197755102F, 0.2230575422F, 0.2263576007F,
  0.2296754753F, 0.2330109540F, 0.2363638225F, 0.2397338646F,
  0.2431208619F, 0.2465245941F, 0.2499448389F, 0.2533813719F,
  0.2568339669F, 0.2603023956F, 0.2637864277F, 0.2672858312F,
  0.2708003718F, 0.2743298135F, 0.2778739186F, 0.2814324472F,
  0.2850051576F, 0.2885918065F, 0.2921921485F, 0.2958059366F,
  0.2994329219F, 0.3030728538F, 0.3067254799F, 0.3103905462F,
  0.3140677969F, 0.3177569747F, 0.3214578205F, 0.3251700736F,
  0.3288934718F, 0.3326277513F, 0.3363726468F, 0.3401278914F,
  0.3438932168F, 0.3476683533F, 0.3514530297F, 0.3552469734F,
  0.3590499106F, 0.3628615659F, 0.3666816630F, 0.3705099239F,
  0.3743460698F, 0.3781898204F, 0.3820408945F, 0.3858990095F,
  0.3897638820F, 0.3936352274F, 0.3975127601F, 0.4013961936F,
  0.4052852405F, 0.4091796123F, 0.4130790198F, 0.4169831732F,
  0.4208917815F, 0.4248045534F, 0.4287211965F, 0.4326414181F,
  0.4365649248F, 0.4404914225F, 0.4444206167F, 0.4483522125F,
  0.4522859146F, 0.4562214270F, 0.4601584538F, 0.4640966984F,
  0.4680358644F, 0.4719756548F, 0.4759157726F, 0.4798559209F,
  0.4837958024F, 0.4877351199F, 0.4916735765F, 0.4956108751F,
  0.4995467188F, 0.5034808109F, 0.5074128550F, 0.5113425550F,
  0.5152696149F, 0.5191937395F, 0.5231146336F, 0.5270320028F,
  0.5309455530F, 0.5348549910F, 0.5387600239F, 0.5426603597F,
  0.5465557070F, 0.5504457754F, 0.5543302752F, 0.5582089175F,
  0.5620814145F, 0.5659474793F, 0.5698068262F, 0.5736591704F,
  0.5775042283F, 0.5813417176F, 0.5851713571F, 0.5889928670F,
  0.5928059689F, 0.5966103856F, 0.6004058415F, 0.6041920626F,
  0.6079687761F, 0.6117357113F, 0.6154925986F, 0.6192391705F,
  0.6229751612F, 0.6267003064F, 0.6304143441F, 0.6341170137F,
  0.6378080569F, 0.6414872173F, 0.6451542405F, 0.6488088741F,
  0.6524508681F, 0.6560799742F, 0.6596959469F, 0.6632985424F,
  0.6668875197F, 0.6704626398F, 0.6740236662F, 0.6775703649F,
  0.6811025043F, 0.6846198554F, 0.6881221916F, 0.6916092892F,
  0.6950809269F, 0.6985368861F, 0.7019769510F, 0.7054009085F,
  0.7088085484F, 0.7121996632F, 0.7155740484F, 0.7189315023F,
  0.7222718263F, 0.7255948245F, 0.7289003043F, 0.7321880760F,
  0.7354579530F, 0.7387097518F, 0.7419432921F, 0.7451583966F,
  0.7483548915F, 0.7515326059F, 0.7546913723F, 0.7578310265F,
  0.7609514077F, 0.7640523581F, 0.7671337237F, 0.7701953535F,
  0.7732371001F, 0.7762588195F, 0.7792603711F, 0.7822416178F,
  0.7852024259F, 0.7881426654F, 0.7910622097F, 0.7939609356F,
  0.7968387237F, 0.7996954579F, 0.8025310261F, 0.8053453193F,
  0.8081382324F, 0.8109096638F, 0.8136595156F, 0.8163876936F,
  0.8190941071F, 0.8217786690F, 0.8244412960F, 0.8270819086F,
  0.8297004305F, 0.8322967896F, 0.8348709171F, 0.8374227481F,
  0.8399522213F, 0.8424592789F, 0.8449438672F, 0.8474059356F,
  0.8498454378F, 0.8522623306F, 0.8546565748F, 0.8570281348F,
  0.8593769787F, 0.8617030779F, 0.8640064080F, 0.8662869477F,
  0.8685446796F, 0.8707795899F, 0.8729916682F, 0.8751809079F,
  0.8773473059F, 0.8794908626F, 0.8816115819F, 0.8837094713F,
  0.8857845418F, 0.8878368079F, 0.8898662874F, 0.8918730019F,
  0.8938569760F, 0.8958182380F, 0.8977568194F, 0.8996727552F,
  0.9015660837F, 0.9034368465F, 0.9052850885F, 0.9071108577F,
  0.9089142057F, 0.9106951869F, 0.9124538591F, 0.9141902832F,
  0.9159045233F, 0.9175966464F, 0.9192667228F, 0.9209148257F,
  0.9225410313F, 0.9241454187F, 0.9257280701F, 0.9272890704F,
  0.9288285075F, 0.9303464720F, 0.9318430576F, 0.9333183603F,
  0.9347724792F, 0.9362055158F, 0.9376175745F, 0.9390087622F,
  0.9403791881F, 0.9417289644F, 0.9430582055F, 0.9443670283F,
  0.9456555521F, 0.9469238986F, 0.9481721917F, 0.9494005577F,
  0.9506091252F, 0.9517980248F, 0.9529673894F, 0.9541173540F,
  0.9552480557F, 0.9563596334F, 0.9574522282F, 0.9585259830F,
  0.9595810428F, 0.9606175542F, 0.9616356656F, 0.9626355274F,
  0.9636172915F, 0.9645811114F, 0.9655271425F, 0.9664555414F,
  0.9673664664F, 0.9682600774F, 0.9691365355F, 0.9699960034F,
  0.9708386448F, 0.9716646250F, 0.9724741103F, 0.9732672685F,
  0.9740442683F, 0.9748052795F, 0.9755504729F, 0.9762800205F,
  0.9769940950F, 0.9776928703F, 0.9783765210F, 0.9790452223F,
  0.9796991504F, 0.9803384823F, 0.9809633954F, 0.9815740679F,
  0.9821706784F, 0.9827534063F, 0.9833224312F, 0.9838779332F,
  0.9844200928F, 0.9849490910F, 0.9854651087F, 0.9859683274F,
  0.9864589286F, 0.9869370940F, 0.9874030054F, 0.9878568447F,
  0.9882987937F, 0.9887290343F, 0.9891477481F, 0.9895551169F,
  0.9899513220F, 0.9903365446F, 0.9907109658F, 0.9910747662F,
  0.9914281260F, 0.9917712252F, 0.9921042433F, 0.9924273593F,
  0.9927407516F, 0.9930445982F, 0.9933390763F, 0.9936243626F,
  0.9939006331F, 0.9941680631F, 0.9944268269F, 0.9946770982F,
  0.9949190498F, 0.9951528537F, 0.9953786808F, 0.9955967011F,
  0.9958070836F, 0.9960099963F, 0.9962056061F, 0.9963940787F,
  0.9965755786F, 0.9967502693F, 0.9969183129F, 0.9970798704F,
  0.9972351013F, 0.9973841640F, 0.9975272151F, 0.9976644103F,
  0.9977959036F, 0.9979218476F, 0.9980423932F, 0.9981576901F,
  0.9982678862F, 0.9983731278F, 0.9984735596F, 0.9985693247F,
  0.9986605645F, 0.9987474186F, 0.9988300248F, 0.9989085193F,
  0.9989830364F, 0.9990537085F, 0.9991206662F, 0.9991840382F,
  0.9992439513F, 0.9993005303F, 0.9993538982F, 0.9994041757F,
  0.9994514817F, 0.9994959330F, 0.9995376444F, 0.9995767286F,
  0.9996132960F, 0.9996474550F, 0.9996793121F, 0.9997089710F,
  0.9997365339F, 0.9997621003F, 0.9997857677F, 0.9998076311F,
  0.9998277836F, 0.9998463156F, 0.9998633155F, 0.9998788692F,
  0.9998930603F, 0.9999059701F, 0.9999176774F, 0.9999282586F,
  0.9999377880F, 0.9999463370F, 0.9999539749F, 0.9999607685F,
  0.9999667820F, 0.9999720773F, 0.9999767136F, 0.9999807479F,
  0.9999842344F, 0.9999872249F, 0.9999897688F, 0.9999919127F,
  0.9999937009F, 0.9999951749F, 0.9999963738F, 0.9999973342F,
  0.9999980900F, 0.9999986724F, 0.9999991103F, 0.9999994297F,
  0.9999996543F, 0.9999998049F, 0.9999999000F, 0.9999999552F,
  0.9999999836F, 0.9999999957F, 0.9999999994F, 1.0000000000F,
};

static const float vwin2048[1024] = {
  0.0000009241F, 0.0000083165F, 0.0000231014F, 0.0000452785F,
  0.0000748476F, 0.0001118085F, 0.0001561608F, 0.0002079041F,
  0.0002670379F, 0.0003335617F, 0.0004074748F, 0.0004887765F,
  0.0005774661F, 0.0006735427F, 0.0007770054F, 0.0008878533F,
  0.0010060853F, 0.0011317002F, 0.0012646969F, 0.0014050742F,
  0.0015528307F, 0.0017079650F, 0.0018704756F, 0.0020403610F,
  0.0022176196F, 0.0024022497F, 0.0025942495F, 0.0027936173F,
  0.0030003511F, 0.0032144490F, 0.0034359088F, 0.0036647286F,
  0.0039009061F, 0.0041444391F, 0.0043953253F, 0.0046535621F,
  0.0049191472F, 0.0051920781F, 0.0054723520F, 0.0057599664F,
  0.0060549184F, 0.0063572052F, 0.0066668239F, 0.0069837715F,
  0.0073080449F, 0.0076396410F, 0.0079785566F, 0.0083247884F,
  0.0086783330F, 0.0090391871F, 0.0094073470F, 0.0097828092F,
  0.0101655700F, 0.0105556258F, 0.0109529726F, 0.0113576065F,
  0.0117695237F, 0.0121887200F, 0.0126151913F, 0.0130489335F,
  0.0134899422F, 0.0139382130F, 0.0143937415F, 0.0148565233F,
  0.0153265536F, 0.0158038279F, 0.0162883413F, 0.0167800889F,
  0.0172790660F, 0.0177852675F, 0.0182986882F, 0.0188193231F,
  0.0193471668F, 0.0198822141F, 0.0204244594F, 0.0209738974F,
  0.0215305225F, 0.0220943289F, 0.0226653109F, 0.0232434627F,
  0.0238287784F, 0.0244212519F, 0.0250208772F, 0.0256276481F,
  0.0262415582F, 0.0268626014F, 0.0274907711F, 0.0281260608F,
  0.0287684638F, 0.0294179736F, 0.0300745833F, 0.0307382859F,
  0.0314090747F, 0.0320869424F, 0.0327718819F, 0.0334638860F,
  0.0341629474F, 0.0348690586F, 0.0355822122F, 0.0363024004F,
  0.0370296157F, 0.0377638502F, 0.0385050960F, 0.0392533451F,
  0.0400085896F, 0.0407708211F, 0.0415400315F, 0.0423162123F,
  0.0430993552F, 0.0438894515F, 0.0446864926F, 0.0454904698F,
  0.0463013742F, 0.0471191969F, 0.0479439288F, 0.0487755607F,
  0.0496140836F, 0.0504594879F, 0.0513117642F, 0.0521709031F,
  0.0530368949F, 0.0539097297F, 0.0547893979F, 0.0556758894F,
  0.0565691941F, 0.0574693019F, 0.0583762026F, 0.0592898858F,
  0.0602103410F, 0.0611375576F, 0.0620715250F, 0.0630122324F,
  0.0639596688F, 0.0649138234F, 0.0658746848F, 0.0668422421F,
  0.0678164838F, 0.0687973985F, 0.0697849746F, 0.0707792005F,
  0.0717800645F, 0.0727875547F, 0.0738016591F, 0.0748223656F,
  0.0758496620F, 0.0768835359F, 0.0779239751F, 0.0789709668F,
  0.0800244985F, 0.0810845574F, 0.0821511306F, 0.0832242052F,
  0.0843037679F, 0.0853898056F, 0.0864823050F, 0.0875812525F,
  0.0886866347F, 0.0897984378F, 0.0909166480F, 0.0920412513F,
  0.0931722338F, 0.0943095813F, 0.0954532795F, 0.0966033140F,
  0.0977596702F, 0.0989223336F, 0.1000912894F, 0.1012665227F,
  0.1024480185F, 0.1036357616F, 0.1048297369F, 0.1060299290F,
  0.1072363224F, 0.1084489014F, 0.1096676504F, 0.1108925534F,
  0.1121235946F, 0.1133607577F, 0.1146040267F, 0.1158533850F,
  0.1171088163F, 0.1183703040F, 0.1196378312F, 0.1209113812F,
  0.1221909370F, 0.1234764815F, 0.1247679974F, 0.1260654674F,
  0.1273688740F, 0.1286781995F, 0.1299934263F, 0.1313145365F,
  0.1326415121F, 0.1339743349F, 0.1353129866F, 0.1366574490F,
  0.1380077035F, 0.1393637315F, 0.1407255141F, 0.1420930325F,
  0.1434662677F, 0.1448452004F, 0.1462298115F, 0.1476200814F,
  0.1490159906F, 0.1504175195F, 0.1518246482F, 0.1532373569F,
  0.1546556253F, 0.1560794333F, 0.1575087606F, 0.1589435866F,
  0.1603838909F, 0.1618296526F, 0.1632808509F, 0.1647374648F,
  0.1661994731F, 0.1676668546F, 0.1691395880F, 0.1706176516F,
  0.1721010238F, 0.1735896829F, 0.1750836068F, 0.1765827736F,
  0.1780871610F, 0.1795967468F, 0.1811115084F, 0.1826314234F,
  0.1841564689F, 0.1856866221F, 0.1872218600F, 0.1887621595F,
  0.1903074974F, 0.1918578503F, 0.1934131947F, 0.1949735068F,
  0.1965387630F, 0.1981089393F, 0.1996840117F, 0.2012639560F,
  0.2028487479F, 0.2044383630F, 0.2060327766F, 0.2076319642F,
  0.2092359007F, 0.2108445614F, 0.2124579211F, 0.2140759545F,
  0.2156986364F, 0.2173259411F, 0.2189578432F, 0.2205943168F,
  0.2222353361F, 0.2238808751F, 0.2255309076F, 0.2271854073F,
  0.2288443480F, 0.2305077030F, 0.2321754457F, 0.2338475493F,
  0.2355239869F, 0.2372047315F, 0.2388897560F, 0.2405790329F,
  0.2422725350F, 0.2439702347F, 0.2456721043F, 0.2473781159F,
  0.2490882418F, 0.2508024539F, 0.2525207240F, 0.2542430237F,
  0.2559693248F, 0.2576995986F, 0.2594338166F, 0.2611719498F,
  0.2629139695F, 0.2646598466F, 0.2664095520F, 0.2681630564F,
  0.2699203304F, 0.2716813445F, 0.2734460691F, 0.2752144744F,
  0.2769865307F, 0.2787622079F, 0.2805414760F, 0.2823243047F,
  0.2841106637F, 0.2859005227F, 0.2876938509F, 0.2894906179F,
  0.2912907928F, 0.2930943447F, 0.2949012426F, 0.2967114554F,
  0.2985249520F, 0.3003417009F, 0.3021616708F, 0.3039848301F,
  0.3058111471F, 0.3076405901F, 0.3094731273F, 0.3113087266F,
  0.3131473560F, 0.3149889833F, 0.3168335762F, 0.3186811024F,
  0.3205315294F, 0.3223848245F, 0.3242409552F, 0.3260998886F,
  0.3279615918F, 0.3298260319F, 0.3316931758F, 0.3335629903F,
  0.3354354423F, 0.3373104982F, 0.3391881247F, 0.3410682882F,
  0.3429509551F, 0.3448360917F, 0.3467236642F, 0.3486136387F,
  0.3505059811F, 0.3524006575F, 0.3542976336F, 0.3561968753F,
  0.3580983482F, 0.3600020179F, 0.3619078499F, 0.3638158096F,
  0.3657258625F, 0.3676379737F, 0.3695521086F, 0.3714682321F,
  0.3733863094F, 0.3753063055F, 0.3772281852F, 0.3791519134F,
  0.3810774548F, 0.3830047742F, 0.3849338362F, 0.3868646053F,
  0.3887970459F, 0.3907311227F, 0.3926667998F, 0.3946040417F,
  0.3965428125F, 0.3984830765F, 0.4004247978F, 0.4023679403F,
  0.4043124683F, 0.4062583455F, 0.4082055359F, 0.4101540034F,
  0.4121037117F, 0.4140546246F, 0.4160067058F, 0.4179599190F,
  0.4199142277F, 0.4218695956F, 0.4238259861F, 0.4257833627F,
  0.4277416888F, 0.4297009279F, 0.4316610433F, 0.4336219983F,
  0.4355837562F, 0.4375462803F, 0.4395095337F, 0.4414734797F,
  0.4434380815F, 0.4454033021F, 0.4473691046F, 0.4493354521F,
  0.4513023078F, 0.4532696345F, 0.4552373954F, 0.4572055533F,
  0.4591740713F, 0.4611429123F, 0.4631120393F, 0.4650814151F,
  0.4670510028F, 0.4690207650F, 0.4709906649F, 0.4729606651F,
  0.4749307287F, 0.4769008185F, 0.4788708972F, 0.4808409279F,
  0.4828108732F, 0.4847806962F, 0.4867503597F, 0.4887198264F,
  0.4906890593F, 0.4926580213F, 0.4946266753F, 0.4965949840F,
  0.4985629105F, 0.5005304176F, 0.5024974683F, 0.5044640255F,
  0.5064300522F, 0.5083955114F, 0.5103603659F, 0.5123245790F,
  0.5142881136F, 0.5162509328F, 0.5182129997F, 0.5201742774F,
  0.5221347290F, 0.5240943178F, 0.5260530070F, 0.5280107598F,
  0.5299675395F, 0.5319233095F, 0.5338780330F, 0.5358316736F,
  0.5377841946F, 0.5397355596F, 0.5416857320F, 0.5436346755F,
  0.5455823538F, 0.5475287304F, 0.5494737691F, 0.5514174337F,
  0.5533596881F, 0.5553004962F, 0.5572398218F, 0.5591776291F,
  0.5611138821F, 0.5630485449F, 0.5649815818F, 0.5669129570F,
  0.5688426349F, 0.5707705799F, 0.5726967564F, 0.5746211290F,
  0.5765436624F, 0.5784643212F, 0.5803830702F, 0.5822998743F,
  0.5842146984F, 0.5861275076F, 0.5880382669F, 0.5899469416F,
  0.5918534968F, 0.5937578981F, 0.5956601107F, 0.5975601004F,
  0.5994578326F, 0.6013532732F, 0.6032463880F, 0.6051371429F,
  0.6070255039F, 0.6089114372F, 0.6107949090F, 0.6126758856F,
  0.6145543334F, 0.6164302191F, 0.6183035092F, 0.6201741706F,
  0.6220421700F, 0.6239074745F, 0.6257700513F, 0.6276298674F,
  0.6294868903F, 0.6313410873F, 0.6331924262F, 0.6350408745F,
  0.6368864001F, 0.6387289710F, 0.6405685552F, 0.6424051209F,
  0.6442386364F, 0.6460690702F, 0.6478963910F, 0.6497205673F,
  0.6515415682F, 0.6533593625F, 0.6551739194F, 0.6569852082F,
  0.6587931984F, 0.6605978593F, 0.6623991609F, 0.6641970728F,
  0.6659915652F, 0.6677826081F, 0.6695701718F, 0.6713542268F,
  0.6731347437F, 0.6749116932F, 0.6766850461F, 0.6784547736F,
  0.6802208469F, 0.6819832374F, 0.6837419164F, 0.6854968559F,
  0.6872480275F, 0.6889954034F, 0.6907389556F, 0.6924786566F,
  0.6942144788F, 0.6959463950F, 0.6976743780F, 0.6993984008F,
  0.7011184365F, 0.7028344587F, 0.7045464407F, 0.7062543564F,
  0.7079581796F, 0.7096578844F, 0.7113534450F, 0.7130448359F,
  0.7147320316F, 0.7164150070F, 0.7180937371F, 0.7197681970F,
  0.7214383620F, 0.7231042077F, 0.7247657098F, 0.7264228443F,
  0.7280755871F, 0.7297239147F, 0.7313678035F, 0.7330072301F,
  0.7346421715F, 0.7362726046F, 0.7378985069F, 0.7395198556F,
  0.7411366285F, 0.7427488034F, 0.7443563584F, 0.7459592717F,
  0.7475575218F, 0.7491510873F, 0.7507399471F, 0.7523240803F,
  0.7539034661F, 0.7554780839F, 0.7570479136F, 0.7586129349F,
  0.7601731279F, 0.7617284730F, 0.7632789506F, 0.7648245416F,
  0.7663652267F, 0.7679009872F, 0.7694318044F, 0.7709576599F,
  0.7724785354F, 0.7739944130F, 0.7755052749F, 0.7770111035F,
  0.7785118815F, 0.7800075916F, 0.7814982170F, 0.7829837410F,
  0.7844641472F, 0.7859394191F, 0.7874095408F, 0.7888744965F,
  0.7903342706F, 0.7917888476F, 0.7932382124F, 0.7946823501F,
  0.7961212460F, 0.7975548855F, 0.7989832544F, 0.8004063386F,
  0.8018241244F, 0.8032365981F, 0.8046437463F, 0.8060455560F,
  0.8074420141F, 0.8088331080F, 0.8102188253F, 0.8115991536F,
  0.8129740810F, 0.8143435957F, 0.8157076861F, 0.8170663409F,
  0.8184195489F, 0.8197672994F, 0.8211095817F, 0.8224463853F,
  0.8237777001F, 0.8251035161F, 0.8264238235F, 0.8277386129F,
  0.8290478750F, 0.8303516008F, 0.8316497814F, 0.8329424083F,
  0.8342294731F, 0.8355109677F, 0.8367868841F, 0.8380572148F,
  0.8393219523F, 0.8405810893F, 0.8418346190F, 0.8430825345F,
  0.8443248294F, 0.8455614974F, 0.8467925323F, 0.8480179285F,
  0.8492376802F, 0.8504517822F, 0.8516602292F, 0.8528630164F,
  0.8540601391F, 0.8552515928F, 0.8564373733F, 0.8576174766F,
  0.8587918990F, 0.8599606368F, 0.8611236868F, 0.8622810460F,
  0.8634327113F, 0.8645786802F, 0.8657189504F, 0.8668535195F,
  0.8679823857F, 0.8691055472F, 0.8702230025F, 0.8713347503F,
  0.8724407896F, 0.8735411194F, 0.8746357394F, 0.8757246489F,
  0.8768078479F, 0.8778853364F, 0.8789571146F, 0.8800231832F,
  0.8810835427F, 0.8821381942F, 0.8831871387F, 0.8842303777F,
  0.8852679127F, 0.8862997456F, 0.8873258784F, 0.8883463132F,
  0.8893610527F, 0.8903700994F, 0.8913734562F, 0.8923711263F,
  0.8933631129F, 0.8943494196F, 0.8953300500F, 0.8963050083F,
  0.8972742985F, 0.8982379249F, 0.8991958922F, 0.9001482052F,
  0.9010948688F, 0.9020358883F, 0.9029712690F, 0.9039010165F,
  0.9048251367F, 0.9057436357F, 0.9066565195F, 0.9075637946F,
  0.9084654678F, 0.9093615456F, 0.9102520353F, 0.9111369440F,
  0.9120162792F, 0.9128900484F, 0.9137582595F, 0.9146209204F,
  0.9154780394F, 0.9163296248F, 0.9171756853F, 0.9180162296F,
  0.9188512667F, 0.9196808057F, 0.9205048559F, 0.9213234270F,
  0.9221365285F, 0.9229441704F, 0.9237463629F, 0.9245431160F,
  0.9253344404F, 0.9261203465F, 0.9269008453F, 0.9276759477F,
  0.9284456648F, 0.9292100080F, 0.9299689889F, 0.9307226190F,
  0.9314709103F, 0.9322138747F, 0.9329515245F, 0.9336838721F,
  0.9344109300F, 0.9351327108F, 0.9358492275F, 0.9365604931F,
  0.9372665208F, 0.9379673239F, 0.9386629160F, 0.9393533107F,
  0.9400385220F, 0.9407185637F, 0.9413934501F, 0.9420631954F,
  0.9427278141F, 0.9433873208F, 0.9440417304F, 0.9446910576F,
  0.9453353176F, 0.9459745255F, 0.9466086968F, 0.9472378469F,
  0.9478619915F, 0.9484811463F, 0.9490953274F, 0.9497045506F,
  0.9503088323F, 0.9509081888F, 0.9515026365F, 0.9520921921F,
  0.9526768723F, 0.9532566940F, 0.9538316742F, 0.9544018300F,
  0.9549671786F, 0.9555277375F, 0.9560835241F, 0.9566345562F,
  0.9571808513F, 0.9577224275F, 0.9582593027F, 0.9587914949F,
  0.9593190225F, 0.9598419038F, 0.9603601571F, 0.9608738012F,
  0.9613828546F, 0.9618873361F, 0.9623872646F, 0.9628826591F,
  0.9633735388F, 0.9638599227F, 0.9643418303F, 0.9648192808F,
  0.9652922939F, 0.9657608890F, 0.9662250860F, 0.9666849046F,
  0.9671403646F, 0.9675914861F, 0.9680382891F, 0.9684807937F,
  0.9689190202F, 0.9693529890F, 0.9697827203F, 0.9702082347F,
  0.9706295529F, 0.9710466953F, 0.9714596828F, 0.9718685362F,
  0.9722732762F, 0.9726739240F, 0.9730705005F, 0.9734630267F,
  0.9738515239F, 0.9742360134F, 0.9746165163F, 0.9749930540F,
  0.9753656481F, 0.9757343198F, 0.9760990909F, 0.9764599829F,
  0.9768170175F, 0.9771702164F, 0.9775196013F, 0.9778651941F,
  0.9782070167F, 0.9785450909F, 0.9788794388F, 0.9792100824F,
  0.9795370437F, 0.9798603449F, 0.9801800080F, 0.9804960554F,
  0.9808085092F, 0.9811173916F, 0.9814227251F, 0.9817245318F,
  0.9820228343F, 0.9823176549F, 0.9826090160F, 0.9828969402F,
  0.9831814498F, 0.9834625674F, 0.9837403156F, 0.9840147169F,
  0.9842857939F, 0.9845535692F, 0.9848180654F, 0.9850793052F,
  0.9853373113F, 0.9855921062F, 0.9858437127F, 0.9860921535F,
  0.9863374512F, 0.9865796287F, 0.9868187085F, 0.9870547136F,
  0.9872876664F, 0.9875175899F, 0.9877445067F, 0.9879684396F,
  0.9881894112F, 0.9884074444F, 0.9886225619F, 0.9888347863F,
  0.9890441404F, 0.9892506468F, 0.9894543284F, 0.9896552077F,
  0.9898533074F, 0.9900486502F, 0.9902412587F, 0.9904311555F,
  0.9906183633F, 0.9908029045F, 0.9909848019F, 0.9911640779F,
  0.9913407550F, 0.9915148557F, 0.9916864025F, 0.9918554179F,
  0.9920219241F, 0.9921859437F, 0.9923474989F, 0.9925066120F,
  0.9926633054F, 0.9928176012F, 0.9929695218F, 0.9931190891F,
  0.9932663254F, 0.9934112527F, 0.9935538932F, 0.9936942686F,
  0.9938324012F, 0.9939683126F, 0.9941020248F, 0.9942335597F,
  0.9943629388F, 0.9944901841F, 0.9946153170F, 0.9947383593F,
  0.9948593325F, 0.9949782579F, 0.9950951572F, 0.9952100516F,
  0.9953229625F, 0.9954339111F, 0.9955429186F, 0.9956500062F,
  0.9957551948F, 0.9958585056F, 0.9959599593F, 0.9960595769F,
  0.9961573792F, 0.9962533869F, 0.9963476206F, 0.9964401009F,
  0.9965308483F, 0.9966198833F, 0.9967072261F, 0.9967928971F,
  0.9968769164F, 0.9969593041F, 0.9970400804F, 0.9971192651F,
  0.9971968781F, 0.9972729391F, 0.9973474680F, 0.9974204842F,
  0.9974920074F, 0.9975620569F, 0.9976306521F, 0.9976978122F,
  0.9977635565F, 0.9978279039F, 0.9978908736F, 0.9979524842F,
  0.9980127547F, 0.9980717037F, 0.9981293499F, 0.9981857116F,
  0.9982408073F, 0.9982946554F, 0.9983472739F, 0.9983986810F,
  0.9984488947F, 0.9984979328F, 0.9985458132F, 0.9985925534F,
  0.9986381711F, 0.9986826838F, 0.9987261086F, 0.9987684630F,
  0.9988097640F, 0.9988500286F, 0.9988892738F, 0.9989275163F,
  0.9989647727F, 0.9990010597F, 0.9990363938F, 0.9990707911F,
  0.9991042679F, 0.9991368404F, 0.9991685244F, 0.9991993358F,
  0.9992292905F, 0.9992584038F, 0.9992866914F, 0.9993141686F,
  0.9993408506F, 0.9993667526F, 0.9993918895F, 0.9994162761F,
  0.9994399273F, 0.9994628576F, 0.9994850815F, 0.9995066133F,
  0.9995274672F, 0.9995476574F, 0.9995671978F, 0.9995861021F,
  0.9996043841F, 0.9996220573F, 0.9996391352F, 0.9996556310F,
  0.9996715579F, 0.9996869288F, 0.9997017568F, 0.9997160543F,
  0.9997298342F, 0.9997431088F, 0.9997558905F, 0.9997681914F,
  0.9997800236F, 0.9997913990F, 0.9998023292F, 0.9998128261F,
  0.9998229009F, 0.9998325650F, 0.9998418296F, 0.9998507058F,
  0.9998592044F, 0.9998673362F, 0.9998751117F, 0.9998825415F,
  0.9998896358F, 0.9998964047F, 0.9999028584F, 0.9999090066F,
  0.9999148590F, 0.9999204253F, 0.9999257148F, 0.9999307368F,
  0.9999355003F, 0.9999400144F, 0.9999442878F, 0.9999483293F,
  0.9999521472F, 0.9999557499F, 0.9999591457F, 0.9999623426F,
  0.9999653483F, 0.9999681708F, 0.9999708175F, 0.9999732959F,
  0.9999756132F, 0.9999777765F, 0.9999797928F, 0.9999816688F,
  0.9999834113F, 0.9999850266F, 0.9999865211F, 0.9999879009F,
  0.9999891721F, 0.9999903405F, 0.9999914118F, 0.9999923914F,
  0.9999932849F, 0.9999940972F, 0.9999948336F, 0.9999954989F,
  0.9999960978F, 0.9999966349F, 0.9999971146F, 0.9999975411F,
  0.9999979185F, 0.9999982507F, 0.9999985414F, 0.9999987944F,
  0.9999990129F, 0.9999992003F, 0.9999993596F, 0.9999994939F,
  0.9999996059F, 0.9999996981F, 0.9999997732F, 0.9999998333F,
  0.9999998805F, 0.9999999170F, 0.9999999444F, 0.9999999643F,
  0.9999999784F, 0.9999999878F, 0.9999999937F, 0.9999999972F,
  0.9999999990F, 0.9999999997F, 1.0000000000F, 1.0000000000F,
};

static const float vwin4096[2048] = {
  0.0000002310F, 0.0000020791F, 0.0000057754F, 0.0000113197F,
  0.0000187121F, 0.0000279526F, 0.0000390412F, 0.0000519777F,
  0.0000667623F, 0.0000833949F, 0.0001018753F, 0.0001222036F,
  0.0001443798F, 0.0001684037F, 0.0001942754F, 0.0002219947F,
  0.0002515616F, 0.0002829761F, 0.0003162380F, 0.0003513472F,
  0.0003883038F, 0.0004271076F, 0.0004677584F, 0.0005102563F,
  0.0005546011F, 0.0006007928F, 0.0006488311F, 0.0006987160F,
  0.0007504474F, 0.0008040251F, 0.0008594490F, 0.0009167191F,
  0.0009758351F, 0.0010367969F, 0.0010996044F, 0.0011642574F,
  0.0012307558F, 0.0012990994F, 0.0013692880F, 0.0014413216F,
  0.0015151998F, 0.0015909226F, 0.0016684898F, 0.0017479011F,
  0.0018291565F, 0.0019122556F, 0.0019971983F, 0.0020839845F,
  0.0021726138F, 0.0022630861F, 0.0023554012F, 0.0024495588F,
  0.0025455588F, 0.0026434008F, 0.0027430847F, 0.0028446103F,
  0.0029479772F, 0.0030531853F, 0.0031602342F, 0.0032691238F,
  0.0033798538F, 0.0034924239F, 0.0036068338F, 0.0037230833F,
  0.0038411721F, 0.0039610999F, 0.0040828664F, 0.0042064714F,
  0.0043319145F, 0.0044591954F, 0.0045883139F, 0.0047192696F,
  0.0048520622F, 0.0049866914F, 0.0051231569F, 0.0052614583F,
  0.0054015953F, 0.0055435676F, 0.0056873748F, 0.0058330166F,
  0.0059804926F, 0.0061298026F, 0.0062809460F, 0.0064339226F,
  0.0065887320F, 0.0067453738F, 0.0069038476F, 0.0070641531F,
  0.0072262899F, 0.0073902575F, 0.0075560556F, 0.0077236838F,
  0.0078931417F, 0.0080644288F, 0.0082375447F, 0.0084124891F,
  0.0085892615F, 0.0087678614F, 0.0089482885F, 0.0091305422F,
  0.0093146223F, 0.0095005281F, 0.0096882592F, 0.0098778153F,
  0.0100691958F, 0.0102624002F, 0.0104574281F, 0.0106542791F,
  0.0108529525F, 0.0110534480F, 0.0112557651F, 0.0114599032F,
  0.0116658618F, 0.0118736405F, 0.0120832387F, 0.0122946560F,
  0.0125078917F, 0.0127229454F, 0.0129398166F, 0.0131585046F,
  0.0133790090F, 0.0136013292F, 0.0138254647F, 0.0140514149F,
  0.0142791792F, 0.0145087572F, 0.0147401481F, 0.0149733515F,
  0.0152083667F, 0.0154451932F, 0.0156838304F, 0.0159242777F,
  0.0161665345F, 0.0164106001F, 0.0166564741F, 0.0169041557F,
  0.0171536443F, 0.0174049393F, 0.0176580401F, 0.0179129461F,
  0.0181696565F, 0.0184281708F, 0.0186884883F, 0.0189506084F,
  0.0192145303F, 0.0194802535F, 0.0197477772F, 0.0200171008F,
  0.0202882236F, 0.0205611449F, 0.0208358639F, 0.0211123801F,
  0.0213906927F, 0.0216708011F, 0.0219527043F, 0.0222364019F,
  0.0225218930F, 0.0228091769F, 0.0230982529F, 0.0233891203F,
  0.0236817782F, 0.0239762259F, 0.0242724628F, 0.0245704880F,
  0.0248703007F, 0.0251719002F, 0.0254752858F, 0.0257804565F,
  0.0260874117F, 0.0263961506F, 0.0267066722F, 0.0270189760F,
  0.0273330609F, 0.0276489263F, 0.0279665712F, 0.0282859949F,
  0.0286071966F, 0.0289301753F, 0.0292549303F, 0.0295814607F,
  0.0299097656F, 0.0302398442F, 0.0305716957F, 0.0309053191F,
  0.0312407135F, 0.0315778782F, 0.0319168122F, 0.0322575145F,
  0.0325999844F, 0.0329442209F, 0.0332902231F, 0.0336379900F,
  0.0339875208F, 0.0343388146F, 0.0346918703F, 0.0350466871F,
  0.0354032640F, 0.0357616000F, 0.0361216943F, 0.0364835458F,
  0.0368471535F, 0.0372125166F, 0.0375796339F, 0.0379485046F,
  0.0383191276F, 0.0386915020F, 0.0390656267F, 0.0394415008F,
  0.0398191231F, 0.0401984927F, 0.0405796086F, 0.0409624698F,
  0.0413470751F, 0.0417334235F, 0.0421215141F, 0.0425113457F,
  0.0429029172F, 0.0432962277F, 0.0436912760F, 0.0440880610F,
  0.0444865817F, 0.0448868370F, 0.0452888257F, 0.0456925468F,
  0.0460979992F, 0.0465051816F, 0.0469140931F, 0.0473247325F,
  0.0477370986F, 0.0481511902F, 0.0485670064F, 0.0489845458F,
  0.0494038074F, 0.0498247899F, 0.0502474922F, 0.0506719131F,
  0.0510980514F, 0.0515259060F, 0.0519554756F, 0.0523867590F,
  0.0528197550F, 0.0532544624F, 0.0536908800F, 0.0541290066F,
  0.0545688408F, 0.0550103815F, 0.0554536274F, 0.0558985772F,
  0.0563452297F, 0.0567935837F, 0.0572436377F, 0.0576953907F,
  0.0581488412F, 0.0586039880F, 0.0590608297F, 0.0595193651F,
  0.0599795929F, 0.0604415117F, 0.0609051202F, 0.0613704170F,
  0.0618374009F, 0.0623060704F, 0.0627764243F, 0.0632484611F,
  0.0637221795F, 0.0641975781F, 0.0646746555F, 0.0651534104F,
  0.0656338413F, 0.0661159469F, 0.0665997257F, 0.0670851763F,
  0.0675722973F, 0.0680610873F, 0.0685515448F, 0.0690436684F,
  0.0695374567F, 0.0700329081F, 0.0705300213F, 0.0710287947F,
  0.0715292269F, 0.0720313163F, 0.0725350616F, 0.0730404612F,
  0.0735475136F, 0.0740562172F, 0.0745665707F, 0.0750785723F,
  0.0755922207F, 0.0761075143F, 0.0766244515F, 0.0771430307F,
  0.0776632505F, 0.0781851092F, 0.0787086052F, 0.0792337371F,
  0.0797605032F, 0.0802889018F, 0.0808189315F, 0.0813505905F,
  0.0818838773F, 0.0824187903F, 0.0829553277F, 0.0834934881F,
  0.0840332697F, 0.0845746708F, 0.0851176899F, 0.0856623252F,
  0.0862085751F, 0.0867564379F, 0.0873059119F, 0.0878569954F,
  0.0884096867F, 0.0889639840F, 0.0895198858F, 0.0900773902F,
  0.0906364955F, 0.0911972000F, 0.0917595019F, 0.0923233995F,
  0.0928888909F, 0.0934559745F, 0.0940246485F, 0.0945949110F,
  0.0951667604F, 0.0957401946F, 0.0963152121F, 0.0968918109F,
  0.0974699893F, 0.0980497454F, 0.0986310773F, 0.0992139832F,
  0.0997984614F, 0.1003845098F, 0.1009721267F, 0.1015613101F,
  0.1021520582F, 0.1027443692F, 0.1033382410F, 0.1039336718F,
  0.1045306597F, 0.1051292027F, 0.1057292990F, 0.1063309466F,
  0.1069341435F, 0.1075388878F, 0.1081451776F, 0.1087530108F,
  0.1093623856F, 0.1099732998F, 0.1105857516F, 0.1111997389F,
  0.1118152597F, 0.1124323121F, 0.1130508939F, 0.1136710032F,
  0.1142926379F, 0.1149157960F, 0.1155404755F, 0.1161666742F,
  0.1167943901F, 0.1174236211F, 0.1180543652F, 0.1186866202F,
  0.1193203841F, 0.1199556548F, 0.1205924300F, 0.1212307078F,
  0.1218704860F, 0.1225117624F, 0.1231545349F, 0.1237988013F,
  0.1244445596F, 0.1250918074F, 0.1257405427F, 0.1263907632F,
  0.1270424667F, 0.1276956512F, 0.1283503142F, 0.1290064537F,
  0.1296640674F, 0.1303231530F, 0.1309837084F, 0.1316457312F,
  0.1323092193F, 0.1329741703F, 0.1336405820F, 0.1343084520F,
  0.1349777782F, 0.1356485582F, 0.1363207897F, 0.1369944704F,
  0.1376695979F, 0.1383461700F, 0.1390241842F, 0.1397036384F,
  0.1403845300F, 0.1410668567F, 0.1417506162F, 0.1424358061F,
  0.1431224240F, 0.1438104674F, 0.1444999341F, 0.1451908216F,
  0.1458831274F, 0.1465768492F, 0.1472719844F, 0.1479685308F,
  0.1486664857F, 0.1493658468F, 0.1500666115F, 0.1507687775F,
  0.1514723422F, 0.1521773031F, 0.1528836577F, 0.1535914035F,
  0.1543005380F, 0.1550110587F, 0.1557229631F, 0.1564362485F,
  0.1571509124F, 0.1578669524F, 0.1585843657F, 0.1593031499F,
  0.1600233024F, 0.1607448205F, 0.1614677017F, 0.1621919433F,
  0.1629175428F, 0.1636444975F, 0.1643728047F, 0.1651024619F,
  0.1658334665F, 0.1665658156F, 0.1672995067F, 0.1680345371F,
  0.1687709041F, 0.1695086050F, 0.1702476372F, 0.1709879978F,
  0.1717296843F, 0.1724726938F, 0.1732170237F, 0.1739626711F,
  0.1747096335F, 0.1754579079F, 0.1762074916F, 0.1769583819F,
  0.1777105760F, 0.1784640710F, 0.1792188642F, 0.1799749529F,
  0.1807323340F, 0.1814910049F, 0.1822509628F, 0.1830122046F,
  0.1837747277F, 0.1845385292F, 0.1853036062F, 0.1860699558F,
  0.1868375751F, 0.1876064613F, 0.1883766114F, 0.1891480226F,
  0.1899206919F, 0.1906946164F, 0.1914697932F, 0.1922462194F,
  0.1930238919F, 0.1938028079F, 0.1945829643F, 0.1953643583F,
  0.1961469868F, 0.1969308468F, 0.1977159353F, 0.1985022494F,
  0.1992897859F, 0.2000785420F, 0.2008685145F, 0.2016597005F,
  0.2024520968F, 0.2032457005F, 0.2040405084F, 0.2048365175F,
  0.2056337247F, 0.2064321269F, 0.2072317211F, 0.2080325041F,
  0.2088344727F, 0.2096376240F, 0.2104419547F, 0.2112474618F,
  0.2120541420F, 0.2128619923F, 0.2136710094F, 0.2144811902F,
  0.2152925315F, 0.2161050301F, 0.2169186829F, 0.2177334866F,
  0.2185494381F, 0.2193665340F, 0.2201847712F, 0.2210041465F,
  0.2218246565F, 0.2226462981F, 0.2234690680F, 0.2242929629F,
  0.2251179796F, 0.2259441147F, 0.2267713650F, 0.2275997272F,
  0.2284291979F, 0.2292597739F, 0.2300914518F, 0.2309242283F,
  0.2317581001F, 0.2325930638F, 0.2334291160F, 0.2342662534F,
  0.2351044727F, 0.2359437703F, 0.2367841431F, 0.2376255875F,
  0.2384681001F, 0.2393116776F, 0.2401563165F, 0.2410020134F,
  0.2418487649F, 0.2426965675F, 0.2435454178F, 0.2443953122F,
  0.2452462474F, 0.2460982199F, 0.2469512262F, 0.2478052628F,
  0.2486603262F, 0.2495164129F, 0.2503735194F, 0.2512316421F,
  0.2520907776F, 0.2529509222F, 0.2538120726F, 0.2546742250F,
  0.2555373760F, 0.2564015219F, 0.2572666593F, 0.2581327845F,
  0.2589998939F, 0.2598679840F, 0.2607370510F, 0.2616070916F,
  0.2624781019F, 0.2633500783F, 0.2642230173F, 0.2650969152F,
  0.2659717684F, 0.2668475731F, 0.2677243257F, 0.2686020226F,
  0.2694806601F, 0.2703602344F, 0.2712407419F, 0.2721221789F,
  0.2730045417F, 0.2738878265F, 0.2747720297F, 0.2756571474F,
  0.2765431760F, 0.2774301117F, 0.2783179508F, 0.2792066895F,
  0.2800963240F, 0.2809868505F, 0.2818782654F, 0.2827705647F,
  0.2836637447F, 0.2845578016F, 0.2854527315F, 0.2863485307F,
  0.2872451953F, 0.2881427215F, 0.2890411055F, 0.2899403433F,
  0.2908404312F, 0.2917413654F, 0.2926431418F, 0.2935457567F,
  0.2944492061F, 0.2953534863F, 0.2962585932F, 0.2971645230F,
  0.2980712717F, 0.2989788356F, 0.2998872105F, 0.3007963927F,
  0.3017063781F, 0.3026171629F, 0.3035287430F, 0.3044411145F,
  0.3053542736F, 0.3062682161F, 0.3071829381F, 0.3080984356F,
  0.3090147047F, 0.3099317413F, 0.3108495414F, 0.3117681011F,
  0.3126874163F, 0.3136074830F, 0.3145282972F, 0.3154498548F,
  0.3163721517F, 0.3172951841F, 0.3182189477F, 0.3191434385F,
  0.3200686525F, 0.3209945856F, 0.3219212336F, 0.3228485927F,
  0.3237766585F, 0.3247054271F, 0.3256348943F, 0.3265650560F,
  0.3274959081F, 0.3284274465F, 0.3293596671F, 0.3302925657F,
  0.3312261382F, 0.3321603804F, 0.3330952882F, 0.3340308574F,
  0.3349670838F, 0.3359039634F, 0.3368414919F, 0.3377796651F,
  0.3387184789F, 0.3396579290F, 0.3405980113F, 0.3415387216F,
  0.3424800556F, 0.3434220091F, 0.3443645779F, 0.3453077578F,
  0.3462515446F, 0.3471959340F, 0.3481409217F, 0.3490865036F,
  0.3500326754F, 0.3509794328F, 0.3519267715F, 0.3528746873F,
  0.3538231759F, 0.3547722330F, 0.3557218544F, 0.3566720357F,
  0.3576227727F, 0.3585740610F, 0.3595258964F, 0.3604782745F,
  0.3614311910F, 0.3623846417F, 0.3633386221F, 0.3642931280F,
  0.3652481549F, 0.3662036987F, 0.3671597548F, 0.3681163191F,
  0.3690733870F, 0.3700309544F, 0.3709890167F, 0.3719475696F,
  0.3729066089F, 0.3738661299F, 0.3748261285F, 0.3757866002F,
  0.3767475406F, 0.3777089453F, 0.3786708100F, 0.3796331302F,
  0.3805959014F, 0.3815591194F, 0.3825227796F, 0.3834868777F,
  0.3844514093F, 0.3854163698F, 0.3863817549F, 0.3873475601F,
  0.3883137810F, 0.3892804131F, 0.3902474521F, 0.3912148933F,
  0.3921827325F, 0.3931509650F, 0.3941195865F, 0.3950885925F,
  0.3960579785F, 0.3970277400F, 0.3979978725F, 0.3989683716F,
  0.3999392328F, 0.4009104516F, 0.4018820234F, 0.4028539438F,
  0.4038262084F, 0.4047988125F, 0.4057717516F, 0.4067450214F,
  0.4077186172F, 0.4086925345F, 0.4096667688F, 0.4106413155F,
  0.4116161703F, 0.4125913284F, 0.4135667854F, 0.4145425368F,
  0.4155185780F, 0.4164949044F, 0.4174715116F, 0.4184483949F,
  0.4194255498F, 0.4204029718F, 0.4213806563F, 0.4223585987F,
  0.4233367946F, 0.4243152392F, 0.4252939281F, 0.4262728566F,
  0.4272520202F, 0.4282314144F, 0.4292110345F, 0.4301908760F,
  0.4311709343F, 0.4321512047F, 0.4331316828F, 0.4341123639F,
  0.4350932435F, 0.4360743168F, 0.4370555794F, 0.4380370267F,
  0.4390186540F, 0.4400004567F, 0.4409824303F, 0.4419645701F,
  0.4429468716F, 0.4439293300F, 0.4449119409F, 0.4458946996F,
  0.4468776014F, 0.4478606418F, 0.4488438162F, 0.4498271199F,
  0.4508105483F, 0.4517940967F, 0.4527777607F, 0.4537615355F,
  0.4547454165F, 0.4557293991F, 0.4567134786F, 0.4576976505F,
  0.4586819101F, 0.4596662527F, 0.4606506738F, 0.4616351687F,
  0.4626197328F, 0.4636043614F, 0.4645890499F, 0.4655737936F,
  0.4665585880F, 0.4675434284F, 0.4685283101F, 0.4695132286F,
  0.4704981791F, 0.4714831570F, 0.4724681577F, 0.4734531766F,
  0.4744382089F, 0.4754232501F, 0.4764082956F, 0.4773933406F,
  0.4783783806F, 0.4793634108F, 0.4803484267F, 0.4813334237F,
  0.4823183969F, 0.4833033419F, 0.4842882540F, 0.4852731285F,
  0.4862579608F, 0.4872427462F, 0.4882274802F, 0.4892121580F,
  0.4901967751F, 0.4911813267F, 0.4921658083F, 0.4931502151F,
  0.4941345427F, 0.4951187863F, 0.4961029412F, 0.4970870029F,
  0.4980709667F, 0.4990548280F, 0.5000385822F, 0.5010222245F,
  0.5020057505F, 0.5029891553F, 0.5039724345F, 0.5049555834F,
  0.5059385973F, 0.5069214716F, 0.5079042018F, 0.5088867831F,
  0.5098692110F, 0.5108514808F, 0.5118335879F, 0.5128155277F,
  0.5137972956F, 0.5147788869F, 0.5157602971F, 0.5167415215F,
  0.5177225555F, 0.5187033945F, 0.5196840339F, 0.5206644692F,
  0.5216446956F, 0.5226247086F, 0.5236045035F, 0.5245840759F,
  0.5255634211F, 0.5265425344F, 0.5275214114F, 0.5285000474F,
  0.5294784378F, 0.5304565781F, 0.5314344637F, 0.5324120899F,
  0.5333894522F, 0.5343665461F, 0.5353433670F, 0.5363199102F,
  0.5372961713F, 0.5382721457F, 0.5392478287F, 0.5402232159F,
  0.5411983027F, 0.5421730845F, 0.5431475569F, 0.5441217151F,
  0.5450955548F, 0.5460690714F, 0.5470422602F, 0.5480151169F,
  0.5489876368F, 0.5499598155F, 0.5509316484F, 0.5519031310F,
  0.5528742587F, 0.5538450271F, 0.5548154317F, 0.5557854680F,
  0.5567551314F, 0.5577244174F, 0.5586933216F, 0.5596618395F,
  0.5606299665F, 0.5615976983F, 0.5625650302F, 0.5635319580F,
  0.5644984770F, 0.5654645828F, 0.5664302709F, 0.5673955370F,
  0.5683603765F, 0.5693247850F, 0.5702887580F, 0.5712522912F,
  0.5722153800F, 0.5731780200F, 0.5741402069F, 0.5751019362F,
  0.5760632034F, 0.5770240042F, 0.5779843341F, 0.5789441889F,
  0.5799035639F, 0.5808624549F, 0.5818208575F, 0.5827787673F,
  0.5837361800F, 0.5846930910F, 0.5856494961F, 0.5866053910F,
  0.5875607712F, 0.5885156324F, 0.5894699703F, 0.5904237804F,
  0.5913770586F, 0.5923298004F, 0.5932820016F, 0.5942336578F,
  0.5951847646F, 0.5961353179F, 0.5970853132F, 0.5980347464F,
  0.5989836131F, 0.5999319090F, 0.6008796298F, 0.6018267713F,
  0.6027733292F, 0.6037192993F, 0.6046646773F, 0.6056094589F,
  0.6065536400F, 0.6074972162F, 0.6084401833F, 0.6093825372F,
  0.6103242736F, 0.6112653884F, 0.6122058772F, 0.6131457359F,
  0.6140849604F, 0.6150235464F, 0.6159614897F, 0.6168987862F,
  0.6178354318F, 0.6187714223F, 0.6197067535F, 0.6206414213F,
  0.6215754215F, 0.6225087501F, 0.6234414028F, 0.6243733757F,
  0.6253046646F, 0.6262352654F, 0.6271651739F, 0.6280943862F,
  0.6290228982F, 0.6299507057F, 0.6308778048F, 0.6318041913F,
  0.6327298612F, 0.6336548105F, 0.6345790352F, 0.6355025312F,
  0.6364252945F, 0.6373473211F, 0.6382686070F, 0.6391891483F,
  0.6401089409F, 0.6410279808F, 0.6419462642F, 0.6428637869F,
  0.6437805452F, 0.6446965350F, 0.6456117524F, 0.6465261935F,
  0.6474398544F, 0.6483527311F, 0.6492648197F, 0.6501761165F,
  0.6510866174F, 0.6519963186F, 0.6529052162F, 0.6538133064F,
  0.6547205854F, 0.6556270492F, 0.6565326941F, 0.6574375162F,
  0.6583415117F, 0.6592446769F, 0.6601470079F, 0.6610485009F,
  0.6619491521F, 0.6628489578F, 0.6637479143F, 0.6646460177F,
  0.6655432643F, 0.6664396505F, 0.6673351724F, 0.6682298264F,
  0.6691236087F, 0.6700165157F, 0.6709085436F, 0.6717996889F,
  0.6726899478F, 0.6735793167F, 0.6744677918F, 0.6753553697F,
  0.6762420466F, 0.6771278190F, 0.6780126832F, 0.6788966357F,
  0.6797796728F, 0.6806617909F, 0.6815429866F, 0.6824232562F,
  0.6833025961F, 0.6841810030F, 0.6850584731F, 0.6859350031F,
  0.6868105894F, 0.6876852284F, 0.6885589168F, 0.6894316510F,
  0.6903034275F, 0.6911742430F, 0.6920440939F, 0.6929129769F,
  0.6937808884F, 0.6946478251F, 0.6955137837F, 0.6963787606F,
  0.6972427525F, 0.6981057560F, 0.6989677678F, 0.6998287845F,
  0.7006888028F, 0.7015478194F, 0.7024058309F, 0.7032628340F,
  0.7041188254F, 0.7049738019F, 0.7058277601F, 0.7066806969F,
  0.7075326089F, 0.7083834929F, 0.7092333457F, 0.7100821640F,
  0.7109299447F, 0.7117766846F, 0.7126223804F, 0.7134670291F,
  0.7143106273F, 0.7151531721F, 0.7159946602F, 0.7168350885F,
  0.7176744539F, 0.7185127534F, 0.7193499837F, 0.7201861418F,
  0.7210212247F, 0.7218552293F, 0.7226881526F, 0.7235199914F,
  0.7243507428F, 0.7251804039F, 0.7260089715F, 0.7268364426F,
  0.7276628144F, 0.7284880839F, 0.7293122481F, 0.7301353040F,
  0.7309572487F, 0.7317780794F, 0.7325977930F, 0.7334163868F,
  0.7342338579F, 0.7350502033F, 0.7358654202F, 0.7366795059F,
  0.7374924573F, 0.7383042718F, 0.7391149465F, 0.7399244787F,
  0.7407328655F, 0.7415401041F, 0.7423461920F, 0.7431511261F,
  0.7439549040F, 0.7447575227F, 0.7455589797F, 0.7463592723F,
  0.7471583976F, 0.7479563532F, 0.7487531363F, 0.7495487443F,
  0.7503431745F, 0.7511364244F, 0.7519284913F, 0.7527193726F,
  0.7535090658F, 0.7542975683F, 0.7550848776F, 0.7558709910F,
  0.7566559062F, 0.7574396205F, 0.7582221314F, 0.7590034366F,
  0.7597835334F, 0.7605624194F, 0.7613400923F, 0.7621165495F,
  0.7628917886F, 0.7636658072F, 0.7644386030F, 0.7652101735F,
  0.7659805164F, 0.7667496292F, 0.7675175098F, 0.7682841556F,
  0.7690495645F, 0.7698137341F, 0.7705766622F, 0.7713383463F,
  0.7720987844F, 0.7728579741F, 0.7736159132F, 0.7743725994F,
  0.7751280306F, 0.7758822046F, 0.7766351192F, 0.7773867722F,
  0.7781371614F, 0.7788862848F, 0.7796341401F, 0.7803807253F,
  0.7811260383F, 0.7818700769F, 0.7826128392F, 0.7833543230F,
  0.7840945263F, 0.7848334471F, 0.7855710833F, 0.7863074330F,
  0.7870424941F, 0.7877762647F, 0.7885087428F, 0.7892399264F,
  0.7899698137F, 0.7906984026F, 0.7914256914F, 0.7921516780F,
  0.7928763607F, 0.7935997375F, 0.7943218065F, 0.7950425661F,
  0.7957620142F, 0.7964801492F, 0.7971969692F, 0.7979124724F,
  0.7986266570F, 0.7993395214F, 0.8000510638F, 0.8007612823F,
  0.8014701754F, 0.8021777413F, 0.8028839784F, 0.8035888849F,
  0.8042924592F, 0.8049946997F, 0.8056956048F, 0.8063951727F,
  0.8070934020F, 0.8077902910F, 0.8084858381F, 0.8091800419F,
  0.8098729007F, 0.8105644130F, 0.8112545774F, 0.8119433922F,
  0.8126308561F, 0.8133169676F, 0.8140017251F, 0.8146851272F,
  0.8153671726F, 0.8160478598F, 0.8167271874F, 0.8174051539F,
  0.8180817582F, 0.8187569986F, 0.8194308741F, 0.8201033831F,
  0.8207745244F, 0.8214442966F, 0.8221126986F, 0.8227797290F,
  0.8234453865F, 0.8241096700F, 0.8247725781F, 0.8254341097F,
  0.8260942636F, 0.8267530385F, 0.8274104334F, 0.8280664470F,
  0.8287210782F, 0.8293743259F, 0.8300261889F, 0.8306766662F,
  0.8313257566F, 0.8319734591F, 0.8326197727F, 0.8332646963F,
  0.8339082288F, 0.8345503692F, 0.8351911167F, 0.8358304700F,
  0.8364684284F, 0.8371049907F, 0.8377401562F, 0.8383739238F,
  0.8390062927F, 0.8396372618F, 0.8402668305F, 0.8408949977F,
  0.8415217626F, 0.8421471245F, 0.8427710823F, 0.8433936354F,
  0.8440147830F, 0.8446345242F, 0.8452528582F, 0.8458697844F,
  0.8464853020F, 0.8470994102F, 0.8477121084F, 0.8483233958F,
  0.8489332718F, 0.8495417356F, 0.8501487866F, 0.8507544243F,
  0.8513586479F, 0.8519614568F, 0.8525628505F, 0.8531628283F,
  0.8537613897F, 0.8543585341F, 0.8549542611F, 0.8555485699F,
  0.8561414603F, 0.8567329315F, 0.8573229832F, 0.8579116149F,
  0.8584988262F, 0.8590846165F, 0.8596689855F, 0.8602519327F,
  0.8608334577F, 0.8614135603F, 0.8619922399F, 0.8625694962F,
  0.8631453289F, 0.8637197377F, 0.8642927222F, 0.8648642821F,
  0.8654344172F, 0.8660031272F, 0.8665704118F, 0.8671362708F,
  0.8677007039F, 0.8682637109F, 0.8688252917F, 0.8693854460F,
  0.8699441737F, 0.8705014745F, 0.8710573485F, 0.8716117953F,
  0.8721648150F, 0.8727164073F, 0.8732665723F, 0.8738153098F,
  0.8743626197F, 0.8749085021F, 0.8754529569F, 0.8759959840F,
  0.8765375835F, 0.8770777553F, 0.8776164996F, 0.8781538162F,
  0.8786897054F, 0.8792241670F, 0.8797572013F, 0.8802888082F,
  0.8808189880F, 0.8813477407F, 0.8818750664F, 0.8824009653F,
  0.8829254375F, 0.8834484833F, 0.8839701028F, 0.8844902961F,
  0.8850090636F, 0.8855264054F, 0.8860423218F, 0.8865568131F,
  0.8870698794F, 0.8875815212F, 0.8880917386F, 0.8886005319F,
  0.8891079016F, 0.8896138479F, 0.8901183712F, 0.8906214719F,
  0.8911231503F, 0.8916234067F, 0.8921222417F, 0.8926196556F,
  0.8931156489F, 0.8936102219F, 0.8941033752F, 0.8945951092F,
  0.8950854244F, 0.8955743212F, 0.8960618003F, 0.8965478621F,
  0.8970325071F, 0.8975157359F, 0.8979975490F, 0.8984779471F,
  0.8989569307F, 0.8994345004F, 0.8999106568F, 0.9003854005F,
  0.9008587323F, 0.9013306526F, 0.9018011623F, 0.9022702619F,
  0.9027379521F, 0.9032042337F, 0.9036691074F, 0.9041325739F,
  0.9045946339F, 0.9050552882F, 0.9055145376F, 0.9059723828F,
  0.9064288246F, 0.9068838638F, 0.9073375013F, 0.9077897379F,
  0.9082405743F, 0.9086900115F, 0.9091380503F, 0.9095846917F,
  0.9100299364F, 0.9104737854F, 0.9109162397F, 0.9113573001F,
  0.9117969675F, 0.9122352430F, 0.9126721275F, 0.9131076219F,
  0.9135417273F, 0.9139744447F, 0.9144057750F, 0.9148357194F,
  0.9152642787F, 0.9156914542F, 0.9161172468F, 0.9165416576F,
  0.9169646877F, 0.9173863382F, 0.9178066102F, 0.9182255048F,
  0.9186430232F, 0.9190591665F, 0.9194739359F, 0.9198873324F,
  0.9202993574F, 0.9207100120F, 0.9211192973F, 0.9215272147F,
  0.9219337653F, 0.9223389504F, 0.9227427713F, 0.9231452290F,
  0.9235463251F, 0.9239460607F, 0.9243444371F, 0.9247414557F,
  0.9251371177F, 0.9255314245F, 0.9259243774F, 0.9263159778F,
  0.9267062270F, 0.9270951264F, 0.9274826774F, 0.9278688814F,
  0.9282537398F, 0.9286372540F, 0.9290194254F, 0.9294002555F,
  0.9297797458F, 0.9301578976F, 0.9305347125F, 0.9309101919F,
  0.9312843373F, 0.9316571503F, 0.9320286323F, 0.9323987849F,
  0.9327676097F, 0.9331351080F, 0.9335012816F, 0.9338661320F,
  0.9342296607F, 0.9345918694F, 0.9349527596F, 0.9353123330F,
  0.9356705911F, 0.9360275357F, 0.9363831683F, 0.9367374905F,
  0.9370905042F, 0.9374422108F, 0.9377926122F, 0.9381417099F,
  0.9384895057F, 0.9388360014F, 0.9391811985F, 0.9395250989F,
  0.9398677043F, 0.9402090165F, 0.9405490371F, 0.9408877680F,
  0.9412252110F, 0.9415613678F, 0.9418962402F, 0.9422298301F,
  0.9425621392F, 0.9428931695F, 0.9432229226F, 0.9435514005F,
  0.9438786050F, 0.9442045381F, 0.9445292014F, 0.9448525971F,
  0.9451747268F, 0.9454955926F, 0.9458151963F, 0.9461335399F,
  0.9464506253F, 0.9467664545F, 0.9470810293F, 0.9473943517F,
  0.9477064238F, 0.9480172474F, 0.9483268246F, 0.9486351573F,
  0.9489422475F, 0.9492480973F, 0.9495527087F, 0.9498560837F,
  0.9501582243F, 0.9504591325F, 0.9507588105F, 0.9510572603F,
  0.9513544839F, 0.9516504834F, 0.9519452609F, 0.9522388186F,
  0.9525311584F, 0.9528222826F, 0.9531121932F, 0.9534008923F,
  0.9536883821F, 0.9539746647F, 0.9542597424F, 0.9545436171F,
  0.9548262912F, 0.9551077667F, 0.9553880459F, 0.9556671309F,
  0.9559450239F, 0.9562217272F, 0.9564972429F, 0.9567715733F,
  0.9570447206F, 0.9573166871F, 0.9575874749F, 0.9578570863F,
  0.9581255236F, 0.9583927890F, 0.9586588849F, 0.9589238134F,
  0.9591875769F, 0.9594501777F, 0.9597116180F, 0.9599719003F,
  0.9602310267F, 0.9604889995F, 0.9607458213F, 0.9610014942F,
  0.9612560206F, 0.9615094028F, 0.9617616433F, 0.9620127443F,
  0.9622627083F, 0.9625115376F, 0.9627592345F, 0.9630058016F,
  0.9632512411F, 0.9634955555F, 0.9637387471F, 0.9639808185F,
  0.9642217720F, 0.9644616100F, 0.9647003349F, 0.9649379493F,
  0.9651744556F, 0.9654098561F, 0.9656441534F, 0.9658773499F,
  0.9661094480F, 0.9663404504F, 0.9665703593F, 0.9667991774F,
  0.9670269071F, 0.9672535509F, 0.9674791114F, 0.9677035909F,
  0.9679269921F, 0.9681493174F, 0.9683705694F, 0.9685907506F,
  0.9688098636F, 0.9690279108F, 0.9692448948F, 0.9694608182F,
  0.9696756836F, 0.9698894934F, 0.9701022503F, 0.9703139569F,
  0.9705246156F, 0.9707342291F, 0.9709428000F, 0.9711503309F,
  0.9713568243F, 0.9715622829F, 0.9717667093F, 0.9719701060F,
  0.9721724757F, 0.9723738210F, 0.9725741446F, 0.9727734490F,
  0.9729717369F, 0.9731690109F, 0.9733652737F, 0.9735605279F,
  0.9737547762F, 0.9739480212F, 0.9741402656F, 0.9743315120F,
  0.9745217631F, 0.9747110216F, 0.9748992901F, 0.9750865714F,
  0.9752728681F, 0.9754581829F, 0.9756425184F, 0.9758258775F,
  0.9760082627F, 0.9761896768F, 0.9763701224F, 0.9765496024F,
  0.9767281193F, 0.9769056760F, 0.9770822751F, 0.9772579193F,
  0.9774326114F, 0.9776063542F, 0.9777791502F, 0.9779510023F,
  0.9781219133F, 0.9782918858F, 0.9784609226F, 0.9786290264F,
  0.9787962000F, 0.9789624461F, 0.9791277676F, 0.9792921671F,
  0.9794556474F, 0.9796182113F, 0.9797798615F, 0.9799406009F,
  0.9801004321F, 0.9802593580F, 0.9804173813F, 0.9805745049F,
  0.9807307314F, 0.9808860637F, 0.9810405046F, 0.9811940568F,
  0.9813467232F, 0.9814985065F, 0.9816494095F, 0.9817994351F,
  0.9819485860F, 0.9820968650F, 0.9822442750F, 0.9823908186F,
  0.9825364988F, 0.9826813184F, 0.9828252801F, 0.9829683868F,
  0.9831106413F, 0.9832520463F, 0.9833926048F, 0.9835323195F,
  0.9836711932F, 0.9838092288F, 0.9839464291F, 0.9840827969F,
  0.9842183351F, 0.9843530464F, 0.9844869337F, 0.9846199998F,
  0.9847522475F, 0.9848836798F, 0.9850142993F, 0.9851441090F,
  0.9852731117F, 0.9854013101F, 0.9855287073F, 0.9856553058F,
  0.9857811087F, 0.9859061188F, 0.9860303388F, 0.9861537717F,
  0.9862764202F, 0.9863982872F, 0.9865193756F, 0.9866396882F,
  0.9867592277F, 0.9868779972F, 0.9869959993F, 0.9871132370F,
  0.9872297131F, 0.9873454304F, 0.9874603918F, 0.9875746001F,
  0.9876880581F, 0.9878007688F, 0.9879127348F, 0.9880239592F,
  0.9881344447F, 0.9882441941F, 0.9883532104F, 0.9884614962F,
  0.9885690546F, 0.9886758883F, 0.9887820001F, 0.9888873930F,
  0.9889920697F, 0.9890960331F, 0.9891992859F, 0.9893018312F,
  0.9894036716F, 0.9895048100F, 0.9896052493F, 0.9897049923F,
  0.9898040418F, 0.9899024006F, 0.9900000717F, 0.9900970577F,
  0.9901933616F, 0.9902889862F, 0.9903839343F, 0.9904782087F,
  0.9905718122F, 0.9906647477F, 0.9907570180F, 0.9908486259F,
  0.9909395742F, 0.9910298658F, 0.9911195034F, 0.9912084899F,
  0.9912968281F, 0.9913845208F, 0.9914715708F, 0.9915579810F,
  0.9916437540F, 0.9917288928F, 0.9918134001F, 0.9918972788F,
  0.9919805316F, 0.9920631613F, 0.9921451707F, 0.9922265626F,
  0.9923073399F, 0.9923875052F, 0.9924670615F, 0.9925460114F,
  0.9926243577F, 0.9927021033F, 0.9927792508F, 0.9928558032F,
  0.9929317631F, 0.9930071333F, 0.9930819167F, 0.9931561158F,
  0.9932297337F, 0.9933027728F, 0.9933752362F, 0.9934471264F,
  0.9935184462F, 0.9935891985F, 0.9936593859F, 0.9937290112F,
  0.9937980771F, 0.9938665864F, 0.9939345418F, 0.9940019460F,
  0.9940688018F, 0.9941351118F, 0.9942008789F, 0.9942661057F,
  0.9943307950F, 0.9943949494F, 0.9944585717F, 0.9945216645F,
  0.9945842307F, 0.9946462728F, 0.9947077936F, 0.9947687957F,
  0.9948292820F, 0.9948892550F, 0.9949487174F, 0.9950076719F,
  0.9950661212F, 0.9951240679F, 0.9951815148F, 0.9952384645F,
  0.9952949196F, 0.9953508828F, 0.9954063568F, 0.9954613442F,
  0.9955158476F, 0.9955698697F, 0.9956234132F, 0.9956764806F,
  0.9957290746F, 0.9957811978F, 0.9958328528F, 0.9958840423F,
  0.9959347688F, 0.9959850351F, 0.9960348435F, 0.9960841969F,
  0.9961330977F, 0.9961815486F, 0.9962295521F, 0.9962771108F,
  0.9963242274F, 0.9963709043F, 0.9964171441F, 0.9964629494F,
  0.9965083228F, 0.9965532668F, 0.9965977840F, 0.9966418768F,
  0.9966855479F, 0.9967287998F, 0.9967716350F, 0.9968140559F,
  0.9968560653F, 0.9968976655F, 0.9969388591F, 0.9969796485F,
  0.9970200363F, 0.9970600250F, 0.9970996170F, 0.9971388149F,
  0.9971776211F, 0.9972160380F, 0.9972540683F, 0.9972917142F,
  0.9973289783F, 0.9973658631F, 0.9974023709F, 0.9974385042F,
  0.9974742655F, 0.9975096571F, 0.9975446816F, 0.9975793413F,
  0.9976136386F, 0.9976475759F, 0.9976811557F, 0.9977143803F,
  0.9977472521F, 0.9977797736F, 0.9978119470F, 0.9978437748F,
  0.9978752593F, 0.9979064029F, 0.9979372079F, 0.9979676768F,
  0.9979978117F, 0.9980276151F, 0.9980570893F, 0.9980862367F,
  0.9981150595F, 0.9981435600F, 0.9981717406F, 0.9981996035F,
  0.9982271511F, 0.9982543856F, 0.9982813093F, 0.9983079246F,
  0.9983342336F, 0.9983602386F, 0.9983859418F, 0.9984113456F,
  0.9984364522F, 0.9984612638F, 0.9984857825F, 0.9985100108F,
  0.9985339507F, 0.9985576044F, 0.9985809743F, 0.9986040624F,
  0.9986268710F, 0.9986494022F, 0.9986716583F, 0.9986936413F,
  0.9987153535F, 0.9987367969F, 0.9987579738F, 0.9987788864F,
  0.9987995366F, 0.9988199267F, 0.9988400587F, 0.9988599348F,
  0.9988795572F, 0.9988989278F, 0.9989180487F, 0.9989369222F,
  0.9989555501F, 0.9989739347F, 0.9989920780F, 0.9990099820F,
  0.9990276487F, 0.9990450803F, 0.9990622787F, 0.9990792460F,
  0.9990959841F, 0.9991124952F, 0.9991287812F, 0.9991448440F,
  0.9991606858F, 0.9991763084F, 0.9991917139F, 0.9992069042F,
  0.9992218813F, 0.9992366471F, 0.9992512035F, 0.9992655525F,
  0.9992796961F, 0.9992936361F, 0.9993073744F, 0.9993209131F,
  0.9993342538F, 0.9993473987F, 0.9993603494F, 0.9993731080F,
  0.9993856762F, 0.9993980559F, 0.9994102490F, 0.9994222573F,
  0.9994340827F, 0.9994457269F, 0.9994571918F, 0.9994684793F,
  0.9994795910F, 0.9994905288F, 0.9995012945F, 0.9995118898F,
  0.9995223165F, 0.9995325765F, 0.9995426713F, 0.9995526029F,
  0.9995623728F, 0.9995719829F, 0.9995814349F, 0.9995907304F,
  0.9995998712F, 0.9996088590F, 0.9996176954F, 0.9996263821F,
  0.9996349208F, 0.9996433132F, 0.9996515609F, 0.9996596656F,
  0.9996676288F, 0.9996754522F, 0.9996831375F, 0.9996906862F,
  0.9996981000F, 0.9997053804F, 0.9997125290F, 0.9997195474F,
  0.9997264371F, 0.9997331998F, 0.9997398369F, 0.9997463500F,
  0.9997527406F, 0.9997590103F, 0.9997651606F, 0.9997711930F,
  0.9997771089F, 0.9997829098F, 0.9997885973F, 0.9997941728F,
  0.9997996378F, 0.9998049936F, 0.9998102419F, 0.9998153839F,
  0.9998204211F, 0.9998253550F, 0.9998301868F, 0.9998349182F,
  0.9998395503F, 0.9998440847F, 0.9998485226F, 0.9998528654F,
  0.9998571146F, 0.9998612713F, 0.9998653370F, 0.9998693130F,
  0.9998732007F, 0.9998770012F, 0.9998807159F, 0.9998843461F,
  0.9998878931F, 0.9998913581F, 0.9998947424F, 0.9998980473F,
  0.9999012740F, 0.9999044237F, 0.9999074976F, 0.9999104971F,
  0.9999134231F, 0.9999162771F, 0.9999190601F, 0.9999217733F,
  0.9999244179F, 0.9999269950F, 0.9999295058F, 0.9999319515F,
  0.9999343332F, 0.9999366519F, 0.9999389088F, 0.9999411050F,
  0.9999432416F, 0.9999453196F, 0.9999473402F, 0.9999493044F,
  0.9999512132F, 0.9999530677F, 0.9999548690F, 0.9999566180F,
  0.9999583157F, 0.9999599633F, 0.9999615616F, 0.9999631116F,
  0.9999646144F, 0.9999660709F, 0.9999674820F, 0.9999688487F,
  0.9999701719F, 0.9999714526F, 0.9999726917F, 0.9999738900F,
  0.9999750486F, 0.9999761682F, 0.9999772497F, 0.9999782941F,
  0.9999793021F, 0.9999802747F, 0.9999812126F, 0.9999821167F,
  0.9999829878F, 0.9999838268F, 0.9999846343F, 0.9999854113F,
  0.9999861584F, 0.9999868765F, 0.9999875664F, 0.9999882287F,
  0.9999888642F, 0.9999894736F, 0.9999900577F, 0.9999906172F,
  0.9999911528F, 0.9999916651F, 0.9999921548F, 0.9999926227F,
  0.9999930693F, 0.9999934954F, 0.9999939015F, 0.9999942883F,
  0.9999946564F, 0.9999950064F, 0.9999953390F, 0.9999956547F,
  0.9999959541F, 0.9999962377F, 0.9999965062F, 0.9999967601F,
  0.9999969998F, 0.9999972260F, 0.9999974392F, 0.9999976399F,
  0.9999978285F, 0.9999980056F, 0.9999981716F, 0.9999983271F,
  0.9999984724F, 0.9999986081F, 0.9999987345F, 0.9999988521F,
  0.9999989613F, 0.9999990625F, 0.9999991562F, 0.9999992426F,
  0.9999993223F, 0.9999993954F, 0.9999994625F, 0.9999995239F,
  0.9999995798F, 0.9999996307F, 0.9999996768F, 0.9999997184F,
  0.9999997559F, 0.9999997895F, 0.9999998195F, 0.9999998462F,
  0.9999998698F, 0.9999998906F, 0.9999999088F, 0.9999999246F,
  0.9999999383F, 0.9999999500F, 0.9999999600F, 0.9999999684F,
  0.9999999754F, 0.9999999811F, 0.9999999858F, 0.9999999896F,
  0.9999999925F, 0.9999999948F, 0.9999999965F, 0.9999999978F,
  0.9999999986F, 0.9999999992F, 0.9999999996F, 0.9999999998F,
  0.9999999999F, 1.0000000000F, 1.0000000000F, 1.0000000000F,
};

static const float vwin8192[4096] = {
  0.0000000578F, 0.0000005198F, 0.0000014438F, 0.0000028299F,
  0.0000046780F, 0.0000069882F, 0.0000097604F, 0.0000129945F,
  0.0000166908F, 0.0000208490F, 0.0000254692F, 0.0000305515F,
  0.0000360958F, 0.0000421021F, 0.0000485704F, 0.0000555006F,
  0.0000628929F, 0.0000707472F, 0.0000790635F, 0.0000878417F,
  0.0000970820F, 0.0001067842F, 0.0001169483F, 0.0001275744F,
  0.0001386625F, 0.0001502126F, 0.0001622245F, 0.0001746984F,
  0.0001876343F, 0.0002010320F, 0.0002148917F, 0.0002292132F,
  0.0002439967F, 0.0002592421F, 0.0002749493F, 0.0002911184F,
  0.0003077493F, 0.0003248421F, 0.0003423967F, 0.0003604132F,
  0.0003788915F, 0.0003978316F, 0.0004172335F, 0.0004370971F,
  0.0004574226F, 0.0004782098F, 0.0004994587F, 0.0005211694F,
  0.0005433418F, 0.0005659759F, 0.0005890717F, 0.0006126292F,
  0.0006366484F, 0.0006611292F, 0.0006860716F, 0.0007114757F,
  0.0007373414F, 0.0007636687F, 0.0007904576F, 0.0008177080F,
  0.0008454200F, 0.0008735935F, 0.0009022285F, 0.0009313250F,
  0.0009608830F, 0.0009909025F, 0.0010213834F, 0.0010523257F,
  0.0010837295F, 0.0011155946F, 0.0011479211F, 0.0011807090F,
  0.0012139582F, 0.0012476687F, 0.0012818405F, 0.0013164736F,
  0.0013515679F, 0.0013871235F, 0.0014231402F, 0.0014596182F,
  0.0014965573F, 0.0015339576F, 0.0015718190F, 0.0016101415F,
  0.0016489251F, 0.0016881698F, 0.0017278754F, 0.0017680421F,
  0.0018086698F, 0.0018497584F, 0.0018913080F, 0.0019333185F,
  0.0019757898F, 0.0020187221F, 0.0020621151F, 0.0021059690F,
  0.0021502837F, 0.0021950591F, 0.0022402953F, 0.0022859921F,
  0.0023321497F, 0.0023787679F, 0.0024258467F, 0.0024733861F,
  0.0025213861F, 0.0025698466F, 0.0026187676F, 0.0026681491F,
  0.0027179911F, 0.0027682935F, 0.0028190562F, 0.0028702794F,
  0.0029219628F, 0.0029741066F, 0.0030267107F, 0.0030797749F,
  0.0031332994F, 0.0031872841F, 0.0032417289F, 0.0032966338F,
  0.0033519988F, 0.0034078238F, 0.0034641089F, 0.0035208539F,
  0.0035780589F, 0.0036357237F, 0.0036938485F, 0.0037524331F,
  0.0038114775F, 0.0038709817F, 0.0039309456F, 0.0039913692F,
  0.0040522524F, 0.0041135953F, 0.0041753978F, 0.0042376599F,
  0.0043003814F, 0.0043635624F, 0.0044272029F, 0.0044913028F,
  0.0045558620F, 0.0046208806F, 0.0046863585F, 0.0047522955F,
  0.0048186919F, 0.0048855473F, 0.0049528619F, 0.0050206356F,
  0.0050888684F, 0.0051575601F, 0.0052267108F, 0.0052963204F,
  0.0053663890F, 0.0054369163F, 0.0055079025F, 0.0055793474F,
  0.0056512510F, 0.0057236133F, 0.0057964342F, 0.0058697137F,
  0.0059434517F, 0.0060176482F, 0.0060923032F, 0.0061674166F,
  0.0062429883F, 0.0063190183F, 0.0063955066F, 0.0064724532F,
  0.0065498579F, 0.0066277207F, 0.0067060416F, 0.0067848205F,
  0.0068640575F, 0.0069437523F, 0.0070239051F, 0.0071045157F,
  0.0071855840F, 0.0072671102F, 0.0073490940F, 0.0074315355F,
  0.0075144345F, 0.0075977911F, 0.0076816052F, 0.0077658768F,
  0.0078506057F, 0.0079357920F, 0.0080214355F, 0.0081075363F,
  0.0081940943F, 0.0082811094F, 0.0083685816F, 0.0084565108F,
  0.0085448970F, 0.0086337401F, 0.0087230401F, 0.0088127969F,
  0.0089030104F, 0.0089936807F, 0.0090848076F, 0.0091763911F,
  0.0092684311F, 0.0093609276F, 0.0094538805F, 0.0095472898F,
  0.0096411554F, 0.0097354772F, 0.0098302552F, 0.0099254894F,
  0.0100211796F, 0.0101173259F, 0.0102139281F, 0.0103109863F,
  0.0104085002F, 0.0105064700F, 0.0106048955F, 0.0107037766F,
  0.0108031133F, 0.0109029056F, 0.0110031534F, 0.0111038565F,
  0.0112050151F, 0.0113066289F, 0.0114086980F, 0.0115112222F,
  0.0116142015F, 0.0117176359F, 0.0118215252F, 0.0119258695F,
  0.0120306686F, 0.0121359225F, 0.0122416312F, 0.0123477944F,
  0.0124544123F, 0.0125614847F, 0.0126690116F, 0.0127769928F,
  0.0128854284F, 0.0129943182F, 0.0131036623F, 0.0132134604F,
  0.0133237126F, 0.0134344188F, 0.0135455790F, 0.0136571929F,
  0.0137692607F, 0.0138817821F, 0.0139947572F, 0.0141081859F,
  0.0142220681F, 0.0143364037F, 0.0144511927F, 0.0145664350F,
  0.0146821304F, 0.0147982791F, 0.0149148808F, 0.0150319355F,
  0.0151494431F, 0.0152674036F, 0.0153858168F, 0.0155046828F,
  0.0156240014F, 0.0157437726F, 0.0158639962F, 0.0159846723F,
  0.0161058007F, 0.0162273814F, 0.0163494142F, 0.0164718991F,
  0.0165948361F, 0.0167182250F, 0.0168420658F, 0.0169663584F,
  0.0170911027F, 0.0172162987F, 0.0173419462F, 0.0174680452F,
  0.0175945956F, 0.0177215974F, 0.0178490504F, 0.0179769545F,
  0.0181053098F, 0.0182341160F, 0.0183633732F, 0.0184930812F,
  0.0186232399F, 0.0187538494F, 0.0188849094F, 0.0190164200F,
  0.0191483809F, 0.0192807923F, 0.0194136539F, 0.0195469656F,
  0.0196807275F, 0.0198149394F, 0.0199496012F, 0.0200847128F,
  0.0202202742F, 0.0203562853F, 0.0204927460F, 0.0206296561F,
  0.0207670157F, 0.0209048245F, 0.0210430826F, 0.0211817899F,
  0.0213209462F, 0.0214605515F, 0.0216006057F, 0.0217411086F,
  0.0218820603F, 0.0220234605F, 0.0221653093F, 0.0223076066F,
  0.0224503521F, 0.0225935459F, 0.0227371879F, 0.0228812779F,
  0.0230258160F, 0.0231708018F, 0.0233162355F, 0.0234621169F,
  0.0236084459F, 0.0237552224F, 0.0239024462F, 0.0240501175F,
  0.0241982359F, 0.0243468015F, 0.0244958141F, 0.0246452736F,
  0.0247951800F, 0.0249455331F, 0.0250963329F, 0.0252475792F,
  0.0253992720F, 0.0255514111F, 0.0257039965F, 0.0258570281F,
  0.0260105057F, 0.0261644293F, 0.0263187987F, 0.0264736139F,
  0.0266288747F, 0.0267845811F, 0.0269407330F, 0.0270973302F,
  0.0272543727F, 0.0274118604F, 0.0275697930F, 0.0277281707F,
  0.0278869932F, 0.0280462604F, 0.0282059723F, 0.0283661287F,
  0.0285267295F, 0.0286877747F, 0.0288492641F, 0.0290111976F,
  0.0291735751F, 0.0293363965F, 0.0294996617F, 0.0296633706F,
  0.0298275231F, 0.0299921190F, 0.0301571583F, 0.0303226409F,
  0.0304885667F, 0.0306549354F, 0.0308217472F, 0.0309890017F,
  0.0311566989F, 0.0313248388F, 0.0314934211F, 0.0316624459F,
  0.0318319128F, 0.0320018220F, 0.0321721732F, 0.0323429663F,
  0.0325142013F, 0.0326858779F, 0.0328579962F, 0.0330305559F,
  0.0332035570F, 0.0333769994F, 0.0335508829F, 0.0337252074F,
  0.0338999728F, 0.0340751790F, 0.0342508259F, 0.0344269134F,
  0.0346034412F, 0.0347804094F, 0.0349578178F, 0.0351356663F,
  0.0353139548F, 0.0354926831F, 0.0356718511F, 0.0358514588F,
  0.0360315059F, 0.0362119924F, 0.0363929182F, 0.0365742831F,
  0.0367560870F, 0.0369383297F, 0.0371210113F, 0.0373041315F,
  0.0374876902F, 0.0376716873F, 0.0378561226F, 0.0380409961F,
  0.0382263077F, 0.0384120571F, 0.0385982443F, 0.0387848691F,
  0.0389719315F, 0.0391594313F, 0.0393473683F, 0.0395357425F,
  0.0397245537F, 0.0399138017F, 0.0401034866F, 0.0402936080F,
  0.0404841660F, 0.0406751603F, 0.0408665909F, 0.0410584576F,
  0.0412507603F, 0.0414434988F, 0.0416366731F, 0.0418302829F,
  0.0420243282F, 0.0422188088F, 0.0424137246F, 0.0426090755F,
  0.0428048613F, 0.0430010819F, 0.0431977371F, 0.0433948269F,
  0.0435923511F, 0.0437903095F, 0.0439887020F, 0.0441875285F,
  0.0443867889F, 0.0445864830F, 0.0447866106F, 0.0449871717F,
  0.0451881661F, 0.0453895936F, 0.0455914542F, 0.0457937477F,
  0.0459964738F, 0.0461996326F, 0.0464032239F, 0.0466072475F,
  0.0468117032F, 0.0470165910F, 0.0472219107F, 0.0474276622F,
  0.0476338452F, 0.0478404597F, 0.0480475056F, 0.0482549827F,
  0.0484628907F, 0.0486712297F, 0.0488799994F, 0.0490891998F,
  0.0492988306F, 0.0495088917F, 0.0497193830F, 0.0499303043F,
  0.0501416554F, 0.0503534363F, 0.0505656468F, 0.0507782867F,
  0.0509913559F, 0.0512048542F, 0.0514187815F, 0.0516331376F,
  0.0518479225F, 0.0520631358F, 0.0522787775F, 0.0524948475F,
  0.0527113455F, 0.0529282715F, 0.0531456252F, 0.0533634066F,
  0.0535816154F, 0.0538002515F, 0.0540193148F, 0.0542388051F,
  0.0544587222F, 0.0546790660F, 0.0548998364F, 0.0551210331F,
  0.0553426561F, 0.0555647051F, 0.0557871801F, 0.0560100807F,
  0.0562334070F, 0.0564571587F, 0.0566813357F, 0.0569059378F,
  0.0571309649F, 0.0573564168F, 0.0575822933F, 0.0578085942F,
  0.0580353195F, 0.0582624689F, 0.0584900423F, 0.0587180396F,
  0.0589464605F, 0.0591753049F, 0.0594045726F, 0.0596342635F,
  0.0598643774F, 0.0600949141F, 0.0603258735F, 0.0605572555F,
  0.0607890597F, 0.0610212862F, 0.0612539346F, 0.0614870049F,
  0.0617204968F, 0.0619544103F, 0.0621887451F, 0.0624235010F,
  0.0626586780F, 0.0628942758F, 0.0631302942F, 0.0633667331F,
  0.0636035923F, 0.0638408717F, 0.0640785710F, 0.0643166901F,
  0.0645552288F, 0.0647941870F, 0.0650335645F, 0.0652733610F,
  0.0655135765F, 0.0657542108F, 0.0659952636F, 0.0662367348F,
  0.0664786242F, 0.0667209316F, 0.0669636570F, 0.0672068000F,
  0.0674503605F, 0.0676943384F, 0.0679387334F, 0.0681835454F,
  0.0684287742F, 0.0686744196F, 0.0689204814F, 0.0691669595F,
  0.0694138536F, 0.0696611637F, 0.0699088894F, 0.0701570307F,
  0.0704055873F, 0.0706545590F, 0.0709039458F, 0.0711537473F,
  0.0714039634F, 0.0716545939F, 0.0719056387F, 0.0721570975F,
  0.0724089702F, 0.0726612565F, 0.0729139563F, 0.0731670694F,
  0.0734205956F, 0.0736745347F, 0.0739288866F, 0.0741836510F,
  0.0744388277F, 0.0746944166F, 0.0749504175F, 0.0752068301F,
  0.0754636543F, 0.0757208899F, 0.0759785367F, 0.0762365946F,
  0.0764950632F, 0.0767539424F, 0.0770132320F, 0.0772729319F,
  0.0775330418F, 0.0777935616F, 0.0780544909F, 0.0783158298F,
  0.0785775778F, 0.0788397349F, 0.0791023009F, 0.0793652755F,
  0.0796286585F, 0.0798924498F, 0.0801566492F, 0.0804212564F,
  0.0806862712F, 0.0809516935F, 0.0812175231F, 0.0814837597F,
  0.0817504031F, 0.0820174532F, 0.0822849097F, 0.0825527724F,
  0.0828210412F, 0.0830897158F, 0.0833587960F, 0.0836282816F,
  0.0838981724F, 0.0841684682F, 0.0844391688F, 0.0847102740F,
  0.0849817835F, 0.0852536973F, 0.0855260150F, 0.0857987364F,
  0.0860718614F, 0.0863453897F, 0.0866193211F, 0.0868936554F,
  0.0871683924F, 0.0874435319F, 0.0877190737F, 0.0879950175F,
  0.0882713632F, 0.0885481105F, 0.0888252592F, 0.0891028091F,
  0.0893807600F, 0.0896591117F, 0.0899378639F, 0.0902170165F,
  0.0904965692F, 0.0907765218F, 0.0910568740F, 0.0913376258F,
  0.0916187767F, 0.0919003268F, 0.0921822756F, 0.0924646230F,
  0.0927473687F, 0.0930305126F, 0.0933140545F, 0.0935979940F,
  0.0938823310F, 0.0941670653F, 0.0944521966F, 0.0947377247F,
  0.0950236494F, 0.0953099704F, 0.0955966876F, 0.0958838007F,
  0.0961713094F, 0.0964592136F, 0.0967475131F, 0.0970362075F,
  0.0973252967F, 0.0976147805F, 0.0979046585F, 0.0981949307F,
  0.0984855967F, 0.0987766563F, 0.0990681093F, 0.0993599555F,
  0.0996521945F, 0.0999448263F, 0.1002378506F, 0.1005312671F,
  0.1008250755F, 0.1011192757F, 0.1014138675F, 0.1017088505F,
  0.1020042246F, 0.1022999895F, 0.1025961450F, 0.1028926909F,
  0.1031896268F, 0.1034869526F, 0.1037846680F, 0.1040827729F,
  0.1043812668F, 0.1046801497F, 0.1049794213F, 0.1052790813F,
  0.1055791294F, 0.1058795656F, 0.1061803894F, 0.1064816006F,
  0.1067831991F, 0.1070851846F, 0.1073875568F, 0.1076903155F,
  0.1079934604F, 0.1082969913F, 0.1086009079F, 0.1089052101F,
  0.1092098975F, 0.1095149699F, 0.1098204270F, 0.1101262687F,
  0.1104324946F, 0.1107391045F, 0.1110460982F, 0.1113534754F,
  0.1116612359F, 0.1119693793F, 0.1122779055F, 0.1125868142F,
  0.1128961052F, 0.1132057781F, 0.1135158328F, 0.1138262690F,
  0.1141370863F, 0.1144482847F, 0.1147598638F, 0.1150718233F,
  0.1153841631F, 0.1156968828F, 0.1160099822F, 0.1163234610F,
  0.1166373190F, 0.1169515559F, 0.1172661714F, 0.1175811654F,
  0.1178965374F, 0.1182122874F, 0.1185284149F, 0.1188449198F,
  0.1191618018F, 0.1194790606F, 0.1197966960F, 0.1201147076F,
  0.1204330953F, 0.1207518587F, 0.1210709976F, 0.1213905118F,
  0.1217104009F, 0.1220306647F, 0.1223513029F, 0.1226723153F,
  0.1229937016F, 0.1233154615F, 0.1236375948F, 0.1239601011F,
  0.1242829803F, 0.1246062319F, 0.1249298559F, 0.1252538518F,
  0.1255782195F, 0.1259029586F, 0.1262280689F, 0.1265535501F,
  0.1268794019F, 0.1272056241F, 0.1275322163F, 0.1278591784F,
  0.1281865099F, 0.1285142108F, 0.1288422805F, 0.1291707190F,
  0.1294995259F, 0.1298287009F, 0.1301582437F, 0.1304881542F,
  0.1308184319F, 0.1311490766F, 0.1314800881F, 0.1318114660F,
  0.1321432100F, 0.1324753200F, 0.1328077955F, 0.1331406364F,
  0.1334738422F, 0.1338074129F, 0.1341413479F, 0.1344756472F,
  0.1348103103F, 0.1351453370F, 0.1354807270F, 0.1358164801F,
  0.1361525959F, 0.1364890741F, 0.1368259145F, 0.1371631167F,
  0.1375006805F, 0.1378386056F, 0.1381768917F, 0.1385155384F,
  0.1388545456F, 0.1391939129F, 0.1395336400F, 0.1398737266F,
  0.1402141724F, 0.1405549772F, 0.1408961406F, 0.1412376623F,
  0.1415795421F, 0.1419217797F, 0.1422643746F, 0.1426073268F,
  0.1429506358F, 0.1432943013F, 0.1436383231F, 0.1439827008F,
  0.1443274342F, 0.1446725229F, 0.1450179667F, 0.1453637652F,
  0.1457099181F, 0.1460564252F, 0.1464032861F, 0.1467505006F,
  0.1470980682F, 0.1474459888F, 0.1477942620F, 0.1481428875F,
  0.1484918651F, 0.1488411942F, 0.1491908748F, 0.1495409065F,
  0.1498912889F, 0.1502420218F, 0.1505931048F, 0.1509445376F,
  0.1512963200F, 0.1516484516F, 0.1520009321F, 0.1523537612F,
  0.1527069385F, 0.1530604638F, 0.1534143368F, 0.1537685571F,
  0.1541231244F, 0.1544780384F, 0.1548332987F, 0.1551889052F,
  0.1555448574F, 0.1559011550F, 0.1562577978F, 0.1566147853F,
  0.1569721173F, 0.1573297935F, 0.1576878135F, 0.1580461771F,
  0.1584048838F, 0.1587639334F, 0.1591233255F, 0.1594830599F,
  0.1598431361F, 0.1602035540F, 0.1605643131F, 0.1609254131F,
  0.1612868537F, 0.1616486346F, 0.1620107555F, 0.1623732160F,
  0.1627360158F, 0.1630991545F, 0.1634626319F, 0.1638264476F,
  0.1641906013F, 0.1645550926F, 0.1649199212F, 0.1652850869F,
  0.1656505892F, 0.1660164278F, 0.1663826024F, 0.1667491127F,
  0.1671159583F, 0.1674831388F, 0.1678506541F, 0.1682185036F,
  0.1685866872F, 0.1689552044F, 0.1693240549F, 0.1696932384F,
  0.1700627545F, 0.1704326029F, 0.1708027833F, 0.1711732952F,
  0.1715441385F, 0.1719153127F, 0.1722868175F, 0.1726586526F,
  0.1730308176F, 0.1734033121F, 0.1737761359F, 0.1741492886F,
  0.1745227698F, 0.1748965792F, 0.1752707164F, 0.1756451812F,
  0.1760199731F, 0.1763950918F, 0.1767705370F, 0.1771463083F,
  0.1775224054F, 0.1778988279F, 0.1782755754F, 0.1786526477F,
  0.1790300444F, 0.1794077651F, 0.1797858094F, 0.1801641771F,
  0.1805428677F, 0.1809218810F, 0.1813012165F, 0.1816808739F,
  0.1820608528F, 0.1824411530F, 0.1828217739F, 0.1832027154F,
  0.1835839770F, 0.1839655584F, 0.1843474592F, 0.1847296790F,
  0.1851122175F, 0.1854950744F, 0.1858782492F, 0.1862617417F,
  0.1866455514F, 0.1870296780F, 0.1874141211F, 0.1877988804F,
  0.1881839555F, 0.1885693461F, 0.1889550517F, 0.1893410721F,
  0.1897274068F, 0.1901140555F, 0.1905010178F, 0.1908882933F,
  0.1912758818F, 0.1916637828F, 0.1920519959F, 0.1924405208F,
  0.1928293571F, 0.1932185044F, 0.1936079625F, 0.1939977308F,
  0.1943878091F, 0.1947781969F, 0.1951688939F, 0.1955598998F,
  0.1959512141F, 0.1963428364F, 0.1967347665F, 0.1971270038F,
  0.1975195482F, 0.1979123990F, 0.1983055561F, 0.1986990190F,
  0.1990927873F, 0.1994868607F, 0.1998812388F, 0.2002759212F,
  0.2006709075F, 0.2010661974F, 0.2014617904F, 0.2018576862F,
  0.2022538844F, 0.2026503847F, 0.2030471865F, 0.2034442897F,
  0.2038416937F, 0.2042393982F, 0.2046374028F, 0.2050357071F,
  0.2054343107F, 0.2058332133F, 0.2062324145F, 0.2066319138F,
  0.2070317110F, 0.2074318055F, 0.2078321970F, 0.2082328852F,
  0.2086338696F, 0.2090351498F, 0.2094367255F, 0.2098385962F,
  0.2102407617F, 0.2106432213F, 0.2110459749F, 0.2114490220F,
  0.2118523621F, 0.2122559950F, 0.2126599202F, 0.2130641373F,
  0.2134686459F, 0.2138734456F, 0.2142785361F, 0.2146839168F,
  0.2150895875F, 0.2154955478F, 0.2159017972F, 0.2163083353F,
  0.2167151617F, 0.2171222761F, 0.2175296780F, 0.2179373670F,
  0.2183453428F, 0.2187536049F, 0.2191621529F, 0.2195709864F,
  0.2199801051F, 0.2203895085F, 0.2207991961F, 0.2212091677F,
  0.2216194228F, 0.2220299610F, 0.2224407818F, 0.2228518850F,
  0.2232632699F, 0.2236749364F, 0.2240868839F, 0.2244991121F,
  0.2249116204F, 0.2253244086F, 0.2257374763F, 0.2261508229F,
  0.2265644481F, 0.2269783514F, 0.2273925326F, 0.2278069911F,
  0.2282217265F, 0.2286367384F, 0.2290520265F, 0.2294675902F,
  0.2298834292F, 0.2302995431F, 0.2307159314F, 0.2311325937F,
  0.2315495297F, 0.2319667388F, 0.2323842207F, 0.2328019749F,
  0.2332200011F, 0.2336382988F, 0.2340568675F, 0.2344757070F,
  0.2348948166F, 0.2353141961F, 0.2357338450F, 0.2361537629F,
  0.2365739493F, 0.2369944038F, 0.2374151261F, 0.2378361156F,
  0.2382573720F, 0.2386788948F, 0.2391006836F, 0.2395227380F,
  0.2399450575F, 0.2403676417F, 0.2407904902F, 0.2412136026F,
  0.2416369783F, 0.2420606171F, 0.2424845185F, 0.2429086820F,
  0.2433331072F, 0.2437577936F, 0.2441827409F, 0.2446079486F,
  0.2450334163F, 0.2454591435F, 0.2458851298F, 0.2463113747F,
  0.2467378779F, 0.2471646389F, 0.2475916573F, 0.2480189325F,
  0.2484464643F, 0.2488742521F, 0.2493022955F, 0.2497305940F,
  0.2501591473F, 0.2505879549F, 0.2510170163F, 0.2514463311F,
  0.2518758989F, 0.2523057193F, 0.2527357916F, 0.2531661157F,
  0.2535966909F, 0.2540275169F, 0.2544585931F, 0.2548899193F,
  0.2553214948F, 0.2557533193F, 0.2561853924F, 0.2566177135F,
  0.2570502822F, 0.2574830981F, 0.2579161608F, 0.2583494697F,
  0.2587830245F, 0.2592168246F, 0.2596508697F, 0.2600851593F,
  0.2605196929F, 0.2609544701F, 0.2613894904F, 0.2618247534F,
  0.2622602586F, 0.2626960055F, 0.2631319938F, 0.2635682230F,
  0.2640046925F, 0.2644414021F, 0.2648783511F, 0.2653155391F,
  0.2657529657F, 0.2661906305F, 0.2666285329F, 0.2670666725F,
  0.2675050489F, 0.2679436616F, 0.2683825101F, 0.2688215940F,
  0.2692609127F, 0.2697004660F, 0.2701402532F, 0.2705802739F,
  0.2710205278F, 0.2714610142F, 0.2719017327F, 0.2723426830F,
  0.2727838644F, 0.2732252766F, 0.2736669191F, 0.2741087914F,
  0.2745508930F, 0.2749932235F, 0.2754357824F, 0.2758785693F,
  0.2763215837F, 0.2767648251F, 0.2772082930F, 0.2776519870F,
  0.2780959066F, 0.2785400513F, 0.2789844207F, 0.2794290143F,
  0.2798738316F, 0.2803188722F, 0.2807641355F, 0.2812096211F,
  0.2816553286F, 0.2821012574F, 0.2825474071F, 0.2829937773F,
  0.2834403673F, 0.2838871768F, 0.2843342053F, 0.2847814523F,
  0.2852289174F, 0.2856765999F, 0.2861244996F, 0.2865726159F,
  0.2870209482F, 0.2874694962F, 0.2879182594F, 0.2883672372F,
  0.2888164293F, 0.2892658350F, 0.2897154540F, 0.2901652858F,
  0.2906153298F, 0.2910655856F, 0.2915160527F, 0.2919667306F,
  0.2924176189F, 0.2928687171F, 0.2933200246F, 0.2937715409F,
  0.2942232657F, 0.2946751984F, 0.2951273386F, 0.2955796856F,
  0.2960322391F, 0.2964849986F, 0.2969379636F, 0.2973911335F,
  0.2978445080F, 0.2982980864F, 0.2987518684F, 0.2992058534F,
  0.2996600409F, 0.3001144305F, 0.3005690217F, 0.3010238139F,
  0.3014788067F, 0.3019339995F, 0.3023893920F, 0.3028449835F,
  0.3033007736F, 0.3037567618F, 0.3042129477F, 0.3046693306F,
  0.3051259102F, 0.3055826859F, 0.3060396572F, 0.3064968236F,
  0.3069541847F, 0.3074117399F, 0.3078694887F, 0.3083274307F,
  0.3087855653F, 0.3092438920F, 0.3097024104F, 0.3101611199F,
  0.3106200200F, 0.3110791103F, 0.3115383902F, 0.3119978592F,
  0.3124575169F, 0.3129173627F, 0.3133773961F, 0.3138376166F,
  0.3142980238F, 0.3147586170F, 0.3152193959F, 0.3156803598F,
  0.3161415084F, 0.3166028410F, 0.3170643573F, 0.3175260566F,
  0.3179879384F, 0.3184500023F, 0.3189122478F, 0.3193746743F,
  0.3198372814F, 0.3203000685F, 0.3207630351F, 0.3212261807F,
  0.3216895048F, 0.3221530069F, 0.3226166865F, 0.3230805430F,
  0.3235445760F, 0.3240087849F, 0.3244731693F, 0.3249377285F,
  0.3254024622F, 0.3258673698F, 0.3263324507F, 0.3267977045F,
  0.3272631306F, 0.3277287286F, 0.3281944978F, 0.3286604379F,
  0.3291265482F, 0.3295928284F, 0.3300592777F, 0.3305258958F,
  0.3309926821F, 0.3314596361F, 0.3319267573F, 0.3323940451F,
  0.3328614990F, 0.3333291186F, 0.3337969033F, 0.3342648525F,
  0.3347329658F, 0.3352012427F, 0.3356696825F, 0.3361382849F,
  0.3366070492F, 0.3370759749F, 0.3375450616F, 0.3380143087F,
  0.3384837156F, 0.3389532819F, 0.3394230071F, 0.3398928905F,
  0.3403629317F, 0.3408331302F, 0.3413034854F, 0.3417739967F,
  0.3422446638F, 0.3427154860F, 0.3431864628F, 0.3436575938F,
  0.3441288782F, 0.3446003158F, 0.3450719058F, 0.3455436478F,
  0.3460155412F, 0.3464875856F, 0.3469597804F, 0.3474321250F,
  0.3479046189F, 0.3483772617F, 0.3488500527F, 0.3493229914F,
  0.3497960774F, 0.3502693100F, 0.3507426887F, 0.3512162131F,
  0.3516898825F, 0.3521636965F, 0.3526376545F, 0.3531117559F,
  0.3535860003F, 0.3540603870F, 0.3545349157F, 0.3550095856F,
  0.3554843964F, 0.3559593474F, 0.3564344381F, 0.3569096680F,
  0.3573850366F, 0.3578605432F, 0.3583361875F, 0.3588119687F,
  0.3592878865F, 0.3597639402F, 0.3602401293F, 0.3607164533F,
  0.3611929117F, 0.3616695038F, 0.3621462292F, 0.3626230873F,
  0.3631000776F, 0.3635771995F, 0.3640544525F, 0.3645318360F,
  0.3650093496F, 0.3654869926F, 0.3659647645F, 0.3664426648F,
  0.3669206930F, 0.3673988484F, 0.3678771306F, 0.3683555390F,
  0.3688340731F, 0.3693127322F, 0.3697915160F, 0.3702704237F,
  0.3707494549F, 0.3712286091F, 0.3717078857F, 0.3721872840F,
  0.3726668037F, 0.3731464441F, 0.3736262047F, 0.3741060850F,
  0.3745860843F, 0.3750662023F, 0.3755464382F, 0.3760267915F,
  0.3765072618F, 0.3769878484F, 0.3774685509F, 0.3779493686F,
  0.3784303010F, 0.3789113475F, 0.3793925076F, 0.3798737809F,
  0.3803551666F, 0.3808366642F, 0.3813182733F, 0.3817999932F,
  0.3822818234F, 0.3827637633F, 0.3832458124F, 0.3837279702F,
  0.3842102360F, 0.3846926093F, 0.3851750897F, 0.3856576764F,
  0.3861403690F, 0.3866231670F, 0.3871060696F, 0.3875890765F,
  0.3880721870F, 0.3885554007F, 0.3890387168F, 0.3895221349F,
  0.3900056544F, 0.3904892748F, 0.3909729955F, 0.3914568160F,
  0.3919407356F, 0.3924247539F, 0.3929088702F, 0.3933930841F,
  0.3938773949F, 0.3943618021F, 0.3948463052F, 0.3953309035F,
  0.3958155966F, 0.3963003838F, 0.3967852646F, 0.3972702385F,
  0.3977553048F, 0.3982404631F, 0.3987257127F, 0.3992110531F,
  0.3996964838F, 0.4001820041F, 0.4006676136F, 0.4011533116F,
  0.4016390976F, 0.4021249710F, 0.4026109313F, 0.4030969779F,
  0.4035831102F, 0.4040693277F, 0.4045556299F, 0.4050420160F,
  0.4055284857F, 0.4060150383F, 0.4065016732F, 0.4069883899F,
  0.4074751879F, 0.4079620665F, 0.4084490252F, 0.4089360635F,
  0.4094231807F, 0.4099103763F, 0.4103976498F, 0.4108850005F,
  0.4113724280F, 0.4118599315F, 0.4123475107F, 0.4128351648F,
  0.4133228934F, 0.4138106959F, 0.4142985716F, 0.4147865201F,
  0.4152745408F, 0.4157626330F, 0.4162507963F, 0.4167390301F,
  0.4172273337F, 0.4177157067F, 0.4182041484F, 0.4186926583F,
  0.4191812359F, 0.4196698805F, 0.4201585915F, 0.4206473685F,
  0.4211362108F, 0.4216251179F, 0.4221140892F, 0.4226031241F,
  0.4230922221F, 0.4235813826F, 0.4240706050F, 0.4245598887F,
  0.4250492332F, 0.4255386379F, 0.4260281022F, 0.4265176256F,
  0.4270072075F, 0.4274968473F, 0.4279865445F, 0.4284762984F,
  0.4289661086F, 0.4294559743F, 0.4299458951F, 0.4304358704F,
  0.4309258996F, 0.4314159822F, 0.4319061175F, 0.4323963050F,
  0.4328865441F, 0.4333768342F, 0.4338671749F, 0.4343575654F,
  0.4348480052F, 0.4353384938F, 0.4358290306F, 0.4363196149F,
  0.4368102463F, 0.4373009241F, 0.4377916478F, 0.4382824168F,
  0.4387732305F, 0.4392640884F, 0.4397549899F, 0.4402459343F,
  0.4407369212F, 0.4412279499F, 0.4417190198F, 0.4422101305F,
  0.4427012813F, 0.4431924717F, 0.4436837010F, 0.4441749686F,
  0.4446662742F, 0.4451576169F, 0.4456489963F, 0.4461404118F,
  0.4466318628F, 0.4471233487F, 0.4476148690F, 0.4481064230F,
  0.4485980103F, 0.4490896302F, 0.4495812821F, 0.4500729654F,
  0.4505646797F, 0.4510564243F, 0.4515481986F, 0.4520400021F,
  0.4525318341F, 0.4530236942F, 0.4535155816F, 0.4540074959F,
  0.4544994365F, 0.4549914028F, 0.4554833941F, 0.4559754100F,
  0.4564674499F, 0.4569595131F, 0.4574515991F, 0.4579437074F,
  0.4584358372F, 0.4589279881F, 0.4594201595F, 0.4599123508F,
  0.4604045615F, 0.4608967908F, 0.4613890383F, 0.4618813034F,
  0.4623735855F, 0.4628658841F, 0.4633581984F, 0.4638505281F,
  0.4643428724F, 0.4648352308F, 0.4653276028F, 0.4658199877F,
  0.4663123849F, 0.4668047940F, 0.4672972143F, 0.4677896451F,
  0.4682820861F, 0.4687745365F, 0.4692669958F, 0.4697594634F,
  0.4702519387F, 0.4707444211F, 0.4712369102F, 0.4717294052F,
  0.4722219056F, 0.4727144109F, 0.4732069204F, 0.4736994336F,
  0.4741919498F, 0.4746844686F, 0.4751769893F, 0.4756695113F,
  0.4761620341F, 0.4766545571F, 0.4771470797F, 0.4776396013F,
  0.4781321213F, 0.4786246392F, 0.4791171544F, 0.4796096663F,
  0.4801021744F, 0.4805946779F, 0.4810871765F, 0.4815796694F,
  0.4820721561F, 0.4825646360F, 0.4830571086F, 0.4835495732F,
  0.4840420293F, 0.4845344763F, 0.4850269136F, 0.4855193407F,
  0.4860117569F, 0.4865041617F, 0.4869965545F, 0.4874889347F,
  0.4879813018F, 0.4884736551F, 0.4889659941F, 0.4894583182F,
  0.4899506268F, 0.4904429193F, 0.4909351952F, 0.4914274538F,
  0.4919196947F, 0.4924119172F, 0.4929041207F, 0.4933963046F,
  0.4938884685F, 0.4943806116F, 0.4948727335F, 0.4953648335F,
  0.4958569110F, 0.4963489656F, 0.4968409965F, 0.4973330032F,
  0.4978249852F, 0.4983169419F, 0.4988088726F, 0.4993007768F,
  0.4997926539F, 0.5002845034F, 0.5007763247F, 0.5012681171F,
  0.5017598801F, 0.5022516132F, 0.5027433157F, 0.5032349871F,
  0.5037266268F, 0.5042182341F, 0.5047098086F, 0.5052013497F,
  0.5056928567F, 0.5061843292F, 0.5066757664F, 0.5071671679F,
  0.5076585330F, 0.5081498613F, 0.5086411520F, 0.5091324047F,
  0.5096236187F, 0.5101147934F, 0.5106059284F, 0.5110970230F,
  0.5115880766F, 0.5120790887F, 0.5125700587F, 0.5130609860F,
  0.5135518700F, 0.5140427102F, 0.5145335059F, 0.5150242566F,
  0.5155149618F, 0.5160056208F, 0.5164962331F, 0.5169867980F,
  0.5174773151F, 0.5179677837F, 0.5184582033F, 0.5189485733F,
  0.5194388931F, 0.5199291621F, 0.5204193798F, 0.5209095455F,
  0.5213996588F, 0.5218897190F, 0.5223797256F, 0.5228696779F,
  0.5233595755F, 0.5238494177F, 0.5243392039F, 0.5248289337F,
  0.5253186063F, 0.5258082213F, 0.5262977781F, 0.5267872760F,
  0.5272767146F, 0.5277660932F, 0.5282554112F, 0.5287446682F,
  0.5292338635F, 0.5297229965F, 0.5302120667F, 0.5307010736F,
  0.5311900164F, 0.5316788947F, 0.5321677079F, 0.5326564554F,
  0.5331451366F, 0.5336337511F, 0.5341222981F, 0.5346107771F,
  0.5350991876F, 0.5355875290F, 0.5360758007F, 0.5365640021F,
  0.5370521327F, 0.5375401920F, 0.5380281792F, 0.5385160939F,
  0.5390039355F, 0.5394917034F, 0.5399793971F, 0.5404670159F,
  0.5409545594F, 0.5414420269F, 0.5419294179F, 0.5424167318F,
  0.5429039680F, 0.5433911261F, 0.5438782053F, 0.5443652051F,
  0.5448521250F, 0.5453389644F, 0.5458257228F, 0.5463123995F,
  0.5467989940F, 0.5472855057F, 0.5477719341F, 0.5482582786F,
  0.5487445387F, 0.5492307137F, 0.5497168031F, 0.5502028063F,
  0.5506887228F, 0.5511745520F, 0.5516602934F, 0.5521459463F,
  0.5526315103F, 0.5531169847F, 0.5536023690F, 0.5540876626F,
  0.5545728649F, 0.5550579755F, 0.5555429937F, 0.5560279189F,
  0.5565127507F, 0.5569974884F, 0.5574821315F, 0.5579666794F,
  0.5584511316F, 0.5589354875F, 0.5594197465F, 0.5599039080F,
  0.5603879716F, 0.5608719367F, 0.5613558026F, 0.5618395689F,
  0.5623232350F, 0.5628068002F, 0.5632902642F, 0.5637736262F,
  0.5642568858F, 0.5647400423F, 0.5652230953F, 0.5657060442F,
  0.5661888883F, 0.5666716272F, 0.5671542603F, 0.5676367870F,
  0.5681192069F, 0.5686015192F, 0.5690837235F, 0.5695658192F,
  0.5700478058F, 0.5705296827F, 0.5710114494F, 0.5714931052F,
  0.5719746497F, 0.5724560822F, 0.5729374023F, 0.5734186094F,
  0.5738997029F, 0.5743806823F, 0.5748615470F, 0.5753422965F,
  0.5758229301F, 0.5763034475F, 0.5767838480F, 0.5772641310F,
  0.5777442960F, 0.5782243426F, 0.5787042700F, 0.5791840778F,
  0.5796637654F, 0.5801433322F, 0.5806227778F, 0.5811021016F,
  0.5815813029F, 0.5820603814F, 0.5825393363F, 0.5830181673F,
  0.5834968737F, 0.5839754549F, 0.5844539105F, 0.5849322399F,
  0.5854104425F, 0.5858885179F, 0.5863664653F, 0.5868442844F,
  0.5873219746F, 0.5877995353F, 0.5882769660F, 0.5887542661F,
  0.5892314351F, 0.5897084724F, 0.5901853776F, 0.5906621500F,
  0.5911387892F, 0.5916152945F, 0.5920916655F, 0.5925679016F,
  0.5930440022F, 0.5935199669F, 0.5939957950F, 0.5944714861F,
  0.5949470396F, 0.5954224550F, 0.5958977317F, 0.5963728692F,
  0.5968478669F, 0.5973227244F, 0.5977974411F, 0.5982720163F,
  0.5987464497F, 0.5992207407F, 0.5996948887F, 0.6001688932F,
  0.6006427537F, 0.6011164696F, 0.6015900405F, 0.6020634657F,
  0.6025367447F, 0.6030098770F, 0.6034828621F, 0.6039556995F,
  0.6044283885F, 0.6049009288F, 0.6053733196F, 0.6058455606F,
  0.6063176512F, 0.6067895909F, 0.6072613790F, 0.6077330152F,
  0.6082044989F, 0.6086758295F, 0.6091470065F, 0.6096180294F,
  0.6100888977F, 0.6105596108F, 0.6110301682F, 0.6115005694F,
  0.6119708139F, 0.6124409011F, 0.6129108305F, 0.6133806017F,
  0.6138502139F, 0.6143196669F, 0.6147889599F, 0.6152580926F,
  0.6157270643F, 0.6161958746F, 0.6166645230F, 0.6171330088F,
  0.6176013317F, 0.6180694910F, 0.6185374863F, 0.6190053171F,
  0.6194729827F, 0.6199404828F, 0.6204078167F, 0.6208749841F,
  0.6213419842F, 0.6218088168F, 0.6222754811F, 0.6227419768F,
  0.6232083032F, 0.6236744600F, 0.6241404465F, 0.6246062622F,
  0.6250719067F, 0.6255373795F, 0.6260026799F, 0.6264678076F,
  0.6269327619F, 0.6273975425F, 0.6278621487F, 0.6283265800F,
  0.6287908361F, 0.6292549163F, 0.6297188201F, 0.6301825471F,
  0.6306460966F, 0.6311094683F, 0.6315726617F, 0.6320356761F,
  0.6324985111F, 0.6329611662F, 0.6334236410F, 0.6338859348F,
  0.6343480472F, 0.6348099777F, 0.6352717257F, 0.6357332909F,
  0.6361946726F, 0.6366558704F, 0.6371168837F, 0.6375777122F,
  0.6380383552F, 0.6384988123F, 0.6389590830F, 0.6394191668F,
  0.6398790631F, 0.6403387716F, 0.6407982916F, 0.6412576228F,
  0.6417167645F, 0.6421757163F, 0.6426344778F, 0.6430930483F,
  0.6435514275F, 0.6440096149F, 0.6444676098F, 0.6449254119F,
  0.6453830207F, 0.6458404356F, 0.6462976562F, 0.6467546820F,
  0.6472115125F, 0.6476681472F, 0.6481245856F, 0.6485808273F,
  0.6490368717F, 0.6494927183F, 0.6499483667F, 0.6504038164F,
  0.6508590670F, 0.6513141178F, 0.6517689684F, 0.6522236185F,
  0.6526780673F, 0.6531323146F, 0.6535863598F, 0.6540402024F,
  0.6544938419F, 0.6549472779F, 0.6554005099F, 0.6558535373F,
  0.6563063598F, 0.6567589769F, 0.6572113880F, 0.6576635927F,
  0.6581155906F, 0.6585673810F, 0.6590189637F, 0.6594703380F,
  0.6599215035F, 0.6603724598F, 0.6608232064F, 0.6612737427F,
  0.6617240684F, 0.6621741829F, 0.6626240859F, 0.6630737767F,
  0.6635232550F, 0.6639725202F, 0.6644215720F, 0.6648704098F,
  0.6653190332F, 0.6657674417F, 0.6662156348F, 0.6666636121F,
  0.6671113731F, 0.6675589174F, 0.6680062445F, 0.6684533538F,
  0.6689002450F, 0.6693469177F, 0.6697933712F, 0.6702396052F,
  0.6706856193F, 0.6711314129F, 0.6715769855F, 0.6720223369F,
  0.6724674664F, 0.6729123736F, 0.6733570581F, 0.6738015194F,
  0.6742457570F, 0.6746897706F, 0.6751335596F, 0.6755771236F,
  0.6760204621F, 0.6764635747F, 0.6769064609F, 0.6773491204F,
  0.6777915525F, 0.6782337570F, 0.6786757332F, 0.6791174809F,
  0.6795589995F, 0.6800002886F, 0.6804413477F, 0.6808821765F,
  0.6813227743F, 0.6817631409F, 0.6822032758F, 0.6826431785F,
  0.6830828485F, 0.6835222855F, 0.6839614890F, 0.6844004585F,
  0.6848391936F, 0.6852776939F, 0.6857159589F, 0.6861539883F,
  0.6865917815F, 0.6870293381F, 0.6874666576F, 0.6879037398F,
  0.6883405840F, 0.6887771899F, 0.6892135571F, 0.6896496850F,
  0.6900855733F, 0.6905212216F, 0.6909566294F, 0.6913917963F,
  0.6918267218F, 0.6922614055F, 0.6926958471F, 0.6931300459F,
  0.6935640018F, 0.6939977141F, 0.6944311825F, 0.6948644066F,
  0.6952973859F, 0.6957301200F, 0.6961626085F, 0.6965948510F,
  0.6970268470F, 0.6974585961F, 0.6978900980F, 0.6983213521F,
  0.6987523580F, 0.6991831154F, 0.6996136238F, 0.7000438828F,
  0.7004738921F, 0.7009036510F, 0.7013331594F, 0.7017624166F,
  0.7021914224F, 0.7026201763F, 0.7030486779F, 0.7034769268F,
  0.7039049226F, 0.7043326648F, 0.7047601531F, 0.7051873870F,
  0.7056143662F, 0.7060410902F, 0.7064675586F, 0.7068937711F,
  0.7073197271F, 0.7077454264F, 0.7081708684F, 0.7085960529F,
  0.7090209793F, 0.7094456474F, 0.7098700566F, 0.7102942066F,
  0.7107180970F, 0.7111417274F, 0.7115650974F, 0.7119882066F,
  0.7124110545F, 0.7128336409F, 0.7132559653F, 0.7136780272F,
  0.7140998264F, 0.7145213624F, 0.7149426348F, 0.7153636433F,
  0.7157843874F, 0.7162048668F, 0.7166250810F, 0.7170450296F,
  0.7174647124F, 0.7178841289F, 0.7183032786F, 0.7187221613F,
  0.7191407765F, 0.7195591239F, 0.7199772030F, 0.7203950135F,
  0.7208125550F, 0.7212298271F, 0.7216468294F, 0.7220635616F,
  0.7224800233F, 0.7228962140F, 0.7233121335F, 0.7237277813F,
  0.7241431571F, 0.7245582604F, 0.7249730910F, 0.7253876484F,
  0.7258019322F, 0.7262159422F, 0.7266296778F, 0.7270431388F,
  0.7274563247F, 0.7278692353F, 0.7282818700F, 0.7286942287F,
  0.7291063108F, 0.7295181160F, 0.7299296440F, 0.7303408944F,
  0.7307518669F, 0.7311625609F, 0.7315729763F, 0.7319831126F,
  0.7323929695F, 0.7328025466F, 0.7332118435F, 0.7336208600F,
  0.7340295955F, 0.7344380499F, 0.7348462226F, 0.7352541134F,
  0.7356617220F, 0.7360690478F, 0.7364760907F, 0.7368828502F,
  0.7372893259F, 0.7376955176F, 0.7381014249F, 0.7385070475F,
  0.7389123849F, 0.7393174368F, 0.7397222029F, 0.7401266829F,
  0.7405308763F, 0.7409347829F, 0.7413384023F, 0.7417417341F,
  0.7421447780F, 0.7425475338F, 0.7429500009F, 0.7433521791F,
  0.7437540681F, 0.7441556674F, 0.7445569769F, 0.7449579960F,
  0.7453587245F, 0.7457591621F, 0.7461593084F, 0.7465591631F,
  0.7469587259F, 0.7473579963F, 0.7477569741F, 0.7481556590F,
  0.7485540506F, 0.7489521486F, 0.7493499526F, 0.7497474623F,
  0.7501446775F, 0.7505415977F, 0.7509382227F, 0.7513345521F,
  0.7517305856F, 0.7521263229F, 0.7525217636F, 0.7529169074F,
  0.7533117541F, 0.7537063032F, 0.7541005545F, 0.7544945076F,
  0.7548881623F, 0.7552815182F, 0.7556745749F, 0.7560673323F,
  0.7564597899F, 0.7568519474F, 0.7572438046F, 0.7576353611F,
  0.7580266166F, 0.7584175708F, 0.7588082235F, 0.7591985741F,
  0.7595886226F, 0.7599783685F, 0.7603678116F, 0.7607569515F,
  0.7611457879F, 0.7615343206F, 0.7619225493F, 0.7623104735F,
  0.7626980931F, 0.7630854078F, 0.7634724171F, 0.7638591209F,
  0.7642455188F, 0.7646316106F, 0.7650173959F, 0.7654028744F,
  0.7657880459F, 0.7661729100F, 0.7665574664F, 0.7669417150F,
  0.7673256553F, 0.7677092871F, 0.7680926100F, 0.7684756239F,
  0.7688583284F, 0.7692407232F, 0.7696228080F, 0.7700045826F,
  0.7703860467F, 0.7707671999F, 0.7711480420F, 0.7715285728F,
  0.7719087918F, 0.7722886989F, 0.7726682938F, 0.7730475762F,
  0.7734265458F, 0.7738052023F, 0.7741835454F, 0.7745615750F,
  0.7749392906F, 0.7753166921F, 0.7756937791F, 0.7760705514F,
  0.7764470087F, 0.7768231508F, 0.7771989773F, 0.7775744880F,
  0.7779496827F, 0.7783245610F, 0.7786991227F, 0.7790733676F,
  0.7794472953F, 0.7798209056F, 0.7801941982F, 0.7805671729F,
  0.7809398294F, 0.7813121675F, 0.7816841869F, 0.7820558873F,
  0.7824272684F, 0.7827983301F, 0.7831690720F, 0.7835394940F,
  0.7839095957F, 0.7842793768F, 0.7846488373F, 0.7850179767F,
  0.7853867948F, 0.7857552914F, 0.7861234663F, 0.7864913191F,
  0.7868588497F, 0.7872260578F, 0.7875929431F, 0.7879595055F,
  0.7883257445F, 0.7886916601F, 0.7890572520F, 0.7894225198F,
  0.7897874635F, 0.7901520827F, 0.7905163772F, 0.7908803468F,
  0.7912439912F, 0.7916073102F, 0.7919703035F, 0.7923329710F,
  0.7926953124F, 0.7930573274F, 0.7934190158F, 0.7937803774F,
  0.7941414120F, 0.7945021193F, 0.7948624991F, 0.7952225511F,
  0.7955822752F, 0.7959416711F, 0.7963007387F, 0.7966594775F,
  0.7970178875F, 0.7973759685F, 0.7977337201F, 0.7980911422F,
  0.7984482346F, 0.7988049970F, 0.7991614292F, 0.7995175310F,
  0.7998733022F, 0.8002287426F, 0.8005838519F, 0.8009386299F,
  0.8012930765F, 0.8016471914F, 0.8020009744F, 0.8023544253F,
  0.8027075438F, 0.8030603298F, 0.8034127831F, 0.8037649035F,
  0.8041166906F, 0.8044681445F, 0.8048192647F, 0.8051700512F,
  0.8055205038F, 0.8058706222F, 0.8062204062F, 0.8065698556F,
  0.8069189702F, 0.8072677499F, 0.8076161944F, 0.8079643036F,
  0.8083120772F, 0.8086595151F, 0.8090066170F, 0.8093533827F,
  0.8096998122F, 0.8100459051F, 0.8103916613F, 0.8107370806F,
  0.8110821628F, 0.8114269077F, 0.8117713151F, 0.8121153849F,
  0.8124591169F, 0.8128025108F, 0.8131455666F, 0.8134882839F,
  0.8138306627F, 0.8141727027F, 0.8145144038F, 0.8148557658F,
  0.8151967886F, 0.8155374718F, 0.8158778154F, 0.8162178192F,
  0.8165574830F, 0.8168968067F, 0.8172357900F, 0.8175744328F,
  0.8179127349F, 0.8182506962F, 0.8185883164F, 0.8189255955F,
  0.8192625332F, 0.8195991295F, 0.8199353840F, 0.8202712967F,
  0.8206068673F, 0.8209420958F, 0.8212769820F, 0.8216115256F,
  0.8219457266F, 0.8222795848F, 0.8226131000F, 0.8229462721F,
  0.8232791009F, 0.8236115863F, 0.8239437280F, 0.8242755260F,
  0.8246069801F, 0.8249380901F, 0.8252688559F, 0.8255992774F,
  0.8259293544F, 0.8262590867F, 0.8265884741F, 0.8269175167F,
  0.8272462141F, 0.8275745663F, 0.8279025732F, 0.8282302344F,
  0.8285575501F, 0.8288845199F, 0.8292111437F, 0.8295374215F,
  0.8298633530F, 0.8301889382F, 0.8305141768F, 0.8308390688F,
  0.8311636141F, 0.8314878124F, 0.8318116637F, 0.8321351678F,
  0.8324583246F, 0.8327811340F, 0.8331035957F, 0.8334257098F,
  0.8337474761F, 0.8340688944F, 0.8343899647F, 0.8347106867F,
  0.8350310605F, 0.8353510857F, 0.8356707624F, 0.8359900904F,
  0.8363090696F, 0.8366276999F, 0.8369459811F, 0.8372639131F,
  0.8375814958F, 0.8378987292F, 0.8382156130F, 0.8385321472F,
  0.8388483316F, 0.8391641662F, 0.8394796508F, 0.8397947853F,
  0.8401095697F, 0.8404240037F, 0.8407380873F, 0.8410518204F,
  0.8413652029F, 0.8416782347F, 0.8419909156F, 0.8423032456F,
  0.8426152245F, 0.8429268523F, 0.8432381289F, 0.8435490541F,
  0.8438596279F, 0.8441698502F, 0.8444797208F, 0.8447892396F,
  0.8450984067F, 0.8454072218F, 0.8457156849F, 0.8460237959F,
  0.8463315547F, 0.8466389612F, 0.8469460154F, 0.8472527170F,
  0.8475590661F, 0.8478650625F, 0.8481707063F, 0.8484759971F,
  0.8487809351F, 0.8490855201F, 0.8493897521F, 0.8496936308F,
  0.8499971564F, 0.8503003286F, 0.8506031474F, 0.8509056128F,
  0.8512077246F, 0.8515094828F, 0.8518108872F, 0.8521119379F,
  0.8524126348F, 0.8527129777F, 0.8530129666F, 0.8533126015F,
  0.8536118822F, 0.8539108087F, 0.8542093809F, 0.8545075988F,
  0.8548054623F, 0.8551029712F, 0.8554001257F, 0.8556969255F,
  0.8559933707F, 0.8562894611F, 0.8565851968F, 0.8568805775F,
  0.8571756034F, 0.8574702743F, 0.8577645902F, 0.8580585509F,
  0.8583521566F, 0.8586454070F, 0.8589383021F, 0.8592308420F,
  0.8595230265F, 0.8598148556F, 0.8601063292F, 0.8603974473F,
  0.8606882098F, 0.8609786167F, 0.8612686680F, 0.8615583636F,
  0.8618477034F, 0.8621366874F, 0.8624253156F, 0.8627135878F,
  0.8630015042F, 0.8632890646F, 0.8635762690F, 0.8638631173F,
  0.8641496096F, 0.8644357457F, 0.8647215257F, 0.8650069495F,
  0.8652920171F, 0.8655767283F, 0.8658610833F, 0.8661450820F,
  0.8664287243F, 0.8667120102F, 0.8669949397F, 0.8672775127F,
  0.8675597293F, 0.8678415894F, 0.8681230929F, 0.8684042398F,
  0.8686850302F, 0.8689654640F, 0.8692455412F, 0.8695252617F,
  0.8698046255F, 0.8700836327F, 0.8703622831F, 0.8706405768F,
  0.8709185138F, 0.8711960940F, 0.8714733174F, 0.8717501840F,
  0.8720266939F, 0.8723028469F, 0.8725786430F, 0.8728540824F,
  0.8731291648F, 0.8734038905F, 0.8736782592F, 0.8739522711F,
  0.8742259261F, 0.8744992242F, 0.8747721653F, 0.8750447496F,
  0.8753169770F, 0.8755888475F, 0.8758603611F, 0.8761315177F,
  0.8764023175F, 0.8766727603F, 0.8769428462F, 0.8772125752F,
  0.8774819474F, 0.8777509626F, 0.8780196209F, 0.8782879224F,
  0.8785558669F, 0.8788234546F, 0.8790906854F, 0.8793575594F,
  0.8796240765F, 0.8798902368F, 0.8801560403F, 0.8804214870F,
  0.8806865768F, 0.8809513099F, 0.8812156863F, 0.8814797059F,
  0.8817433687F, 0.8820066749F, 0.8822696243F, 0.8825322171F,
  0.8827944532F, 0.8830563327F, 0.8833178556F, 0.8835790219F,
  0.8838398316F, 0.8841002848F, 0.8843603815F, 0.8846201217F,
  0.8848795054F, 0.8851385327F, 0.8853972036F, 0.8856555182F,
  0.8859134764F, 0.8861710783F, 0.8864283239F, 0.8866852133F,
  0.8869417464F, 0.8871979234F, 0.8874537443F, 0.8877092090F,
  0.8879643177F, 0.8882190704F, 0.8884734671F, 0.8887275078F,
  0.8889811927F, 0.8892345216F, 0.8894874948F, 0.8897401122F,
  0.8899923738F, 0.8902442798F, 0.8904958301F, 0.8907470248F,
  0.8909978640F, 0.8912483477F, 0.8914984759F, 0.8917482487F,
  0.8919976662F, 0.8922467284F, 0.8924954353F, 0.8927437871F,
  0.8929917837F, 0.8932394252F, 0.8934867118F, 0.8937336433F,
  0.8939802199F, 0.8942264417F, 0.8944723087F, 0.8947178210F,
  0.8949629785F, 0.8952077815F, 0.8954522299F, 0.8956963239F,
  0.8959400634F, 0.8961834486F, 0.8964264795F, 0.8966691561F,
  0.8969114786F, 0.8971534470F, 0.8973950614F, 0.8976363219F,
  0.8978772284F, 0.8981177812F, 0.8983579802F, 0.8985978256F,
  0.8988373174F, 0.8990764556F, 0.8993152405F, 0.8995536720F,
  0.8997917502F, 0.9000294751F, 0.9002668470F, 0.9005038658F,
  0.9007405317F, 0.9009768446F, 0.9012128048F, 0.9014484123F,
  0.9016836671F, 0.9019185693F, 0.9021531191F, 0.9023873165F,
  0.9026211616F, 0.9028546546F, 0.9030877954F, 0.9033205841F,
  0.9035530210F, 0.9037851059F, 0.9040168392F, 0.9042482207F,
  0.9044792507F, 0.9047099293F, 0.9049402564F, 0.9051702323F,
  0.9053998569F, 0.9056291305F, 0.9058580531F, 0.9060866248F,
  0.9063148457F, 0.9065427159F, 0.9067702355F, 0.9069974046F,
  0.9072242233F, 0.9074506917F, 0.9076768100F, 0.9079025782F,
  0.9081279964F, 0.9083530647F, 0.9085777833F, 0.9088021523F,
  0.9090261717F, 0.9092498417F, 0.9094731623F, 0.9096961338F,
  0.9099187561F, 0.9101410295F, 0.9103629540F, 0.9105845297F,
  0.9108057568F, 0.9110266354F, 0.9112471656F, 0.9114673475F,
  0.9116871812F, 0.9119066668F, 0.9121258046F, 0.9123445945F,
  0.9125630367F, 0.9127811314F, 0.9129988786F, 0.9132162785F,
  0.9134333312F, 0.9136500368F, 0.9138663954F, 0.9140824073F,
  0.9142980724F, 0.9145133910F, 0.9147283632F, 0.9149429890F,
  0.9151572687F, 0.9153712023F, 0.9155847900F, 0.9157980319F,
  0.9160109282F, 0.9162234790F, 0.9164356844F, 0.9166475445F,
  0.9168590595F, 0.9170702296F, 0.9172810548F, 0.9174915354F,
  0.9177016714F, 0.9179114629F, 0.9181209102F, 0.9183300134F,
  0.9185387726F, 0.9187471879F, 0.9189552595F, 0.9191629876F,
  0.9193703723F, 0.9195774136F, 0.9197841119F, 0.9199904672F,
  0.9201964797F, 0.9204021495F, 0.9206074767F, 0.9208124616F,
  0.9210171043F, 0.9212214049F, 0.9214253636F, 0.9216289805F,
  0.9218322558F, 0.9220351896F, 0.9222377821F, 0.9224400335F,
  0.9226419439F, 0.9228435134F, 0.9230447423F, 0.9232456307F,
  0.9234461787F, 0.9236463865F, 0.9238462543F, 0.9240457822F,
  0.9242449704F, 0.9244438190F, 0.9246423282F, 0.9248404983F,
  0.9250383293F, 0.9252358214F, 0.9254329747F, 0.9256297896F,
  0.9258262660F, 0.9260224042F, 0.9262182044F, 0.9264136667F,
  0.9266087913F, 0.9268035783F, 0.9269980280F, 0.9271921405F,
  0.9273859160F, 0.9275793546F, 0.9277724566F, 0.9279652221F,
  0.9281576513F, 0.9283497443F, 0.9285415014F, 0.9287329227F,
  0.9289240084F, 0.9291147586F, 0.9293051737F, 0.9294952536F,
  0.9296849987F, 0.9298744091F, 0.9300634850F, 0.9302522266F,
  0.9304406340F, 0.9306287074F, 0.9308164471F, 0.9310038532F,
  0.9311909259F, 0.9313776654F, 0.9315640719F, 0.9317501455F,
  0.9319358865F, 0.9321212951F, 0.9323063713F, 0.9324911155F,
  0.9326755279F, 0.9328596085F, 0.9330433577F, 0.9332267756F,
  0.9334098623F, 0.9335926182F, 0.9337750434F, 0.9339571380F,
  0.9341389023F, 0.9343203366F, 0.9345014409F, 0.9346822155F,
  0.9348626606F, 0.9350427763F, 0.9352225630F, 0.9354020207F,
  0.9355811498F, 0.9357599503F, 0.9359384226F, 0.9361165667F,
  0.9362943830F, 0.9364718716F, 0.9366490327F, 0.9368258666F,
  0.9370023733F, 0.9371785533F, 0.9373544066F, 0.9375299335F,
  0.9377051341F, 0.9378800087F, 0.9380545576F, 0.9382287809F,
  0.9384026787F, 0.9385762515F, 0.9387494993F, 0.9389224223F,
  0.9390950209F, 0.9392672951F, 0.9394392453F, 0.9396108716F,
  0.9397821743F, 0.9399531536F, 0.9401238096F, 0.9402941427F,
  0.9404641530F, 0.9406338407F, 0.9408032061F, 0.9409722495F,
  0.9411409709F, 0.9413093707F, 0.9414774491F, 0.9416452062F,
  0.9418126424F, 0.9419797579F, 0.9421465528F, 0.9423130274F,
  0.9424791819F, 0.9426450166F, 0.9428105317F, 0.9429757274F,
  0.9431406039F, 0.9433051616F, 0.9434694005F, 0.9436333209F,
  0.9437969232F, 0.9439602074F, 0.9441231739F, 0.9442858229F,
  0.9444481545F, 0.9446101691F, 0.9447718669F, 0.9449332481F,
  0.9450943129F, 0.9452550617F, 0.9454154945F, 0.9455756118F,
  0.9457354136F, 0.9458949003F, 0.9460540721F, 0.9462129292F,
  0.9463714719F, 0.9465297003F, 0.9466876149F, 0.9468452157F,
  0.9470025031F, 0.9471594772F, 0.9473161384F, 0.9474724869F,
  0.9476285229F, 0.9477842466F, 0.9479396584F, 0.9480947585F,
  0.9482495470F, 0.9484040243F, 0.9485581906F, 0.9487120462F,
  0.9488655913F, 0.9490188262F, 0.9491717511F, 0.9493243662F,
  0.9494766718F, 0.9496286683F, 0.9497803557F, 0.9499317345F,
  0.9500828047F, 0.9502335668F, 0.9503840209F, 0.9505341673F,
  0.9506840062F, 0.9508335380F, 0.9509827629F, 0.9511316810F,
  0.9512802928F, 0.9514285984F, 0.9515765982F, 0.9517242923F,
  0.9518716810F, 0.9520187646F, 0.9521655434F, 0.9523120176F,
  0.9524581875F, 0.9526040534F, 0.9527496154F, 0.9528948739F,
  0.9530398292F, 0.9531844814F, 0.9533288310F, 0.9534728780F,
  0.9536166229F, 0.9537600659F, 0.9539032071F, 0.9540460470F,
  0.9541885858F, 0.9543308237F, 0.9544727611F, 0.9546143981F,
  0.9547557351F, 0.9548967723F, 0.9550375100F, 0.9551779485F,
  0.9553180881F, 0.9554579290F, 0.9555974714F, 0.9557367158F,
  0.9558756623F, 0.9560143112F, 0.9561526628F, 0.9562907174F,
  0.9564284752F, 0.9565659366F, 0.9567031017F, 0.9568399710F,
  0.9569765446F, 0.9571128229F, 0.9572488061F, 0.9573844944F,
  0.9575198883F, 0.9576549879F, 0.9577897936F, 0.9579243056F,
  0.9580585242F, 0.9581924497F, 0.9583260824F, 0.9584594226F,
  0.9585924705F, 0.9587252264F, 0.9588576906F, 0.9589898634F,
  0.9591217452F, 0.9592533360F, 0.9593846364F, 0.9595156465F,
  0.9596463666F, 0.9597767971F, 0.9599069382F, 0.9600367901F,
  0.9601663533F, 0.9602956279F, 0.9604246143F, 0.9605533128F,
  0.9606817236F, 0.9608098471F, 0.9609376835F, 0.9610652332F,
  0.9611924963F, 0.9613194733F, 0.9614461644F, 0.9615725699F,
  0.9616986901F, 0.9618245253F, 0.9619500757F, 0.9620753418F,
  0.9622003238F, 0.9623250219F, 0.9624494365F, 0.9625735679F,
  0.9626974163F, 0.9628209821F, 0.9629442656F, 0.9630672671F,
  0.9631899868F, 0.9633124251F, 0.9634345822F, 0.9635564585F,
  0.9636780543F, 0.9637993699F, 0.9639204056F, 0.9640411616F,
  0.9641616383F, 0.9642818359F, 0.9644017549F, 0.9645213955F,
  0.9646407579F, 0.9647598426F, 0.9648786497F, 0.9649971797F,
  0.9651154328F, 0.9652334092F, 0.9653511095F, 0.9654685337F,
  0.9655856823F, 0.9657025556F, 0.9658191538F, 0.9659354773F,
  0.9660515263F, 0.9661673013F, 0.9662828024F, 0.9663980300F,
  0.9665129845F, 0.9666276660F, 0.9667420750F, 0.9668562118F,
  0.9669700766F, 0.9670836698F, 0.9671969917F, 0.9673100425F,
  0.9674228227F, 0.9675353325F, 0.9676475722F, 0.9677595422F,
  0.9678712428F, 0.9679826742F, 0.9680938368F, 0.9682047309F,
  0.9683153569F, 0.9684257150F, 0.9685358056F, 0.9686456289F,
  0.9687551853F, 0.9688644752F, 0.9689734987F, 0.9690822564F,
  0.9691907483F, 0.9692989750F, 0.9694069367F, 0.9695146337F,
  0.9696220663F, 0.9697292349F, 0.9698361398F, 0.9699427813F,
  0.9700491597F, 0.9701552754F, 0.9702611286F, 0.9703667197F,
  0.9704720490F, 0.9705771169F, 0.9706819236F, 0.9707864695F,
  0.9708907549F, 0.9709947802F, 0.9710985456F, 0.9712020514F,
  0.9713052981F, 0.9714082859F, 0.9715110151F, 0.9716134862F,
  0.9717156993F, 0.9718176549F, 0.9719193532F, 0.9720207946F,
  0.9721219794F, 0.9722229080F, 0.9723235806F, 0.9724239976F,
  0.9725241593F, 0.9726240661F, 0.9727237183F, 0.9728231161F,
  0.9729222601F, 0.9730211503F, 0.9731197873F, 0.9732181713F,
  0.9733163027F, 0.9734141817F, 0.9735118088F, 0.9736091842F,
  0.9737063083F, 0.9738031814F, 0.9738998039F, 0.9739961760F,
  0.9740922981F, 0.9741881706F, 0.9742837938F, 0.9743791680F,
  0.9744742935F, 0.9745691707F, 0.9746637999F, 0.9747581814F,
  0.9748523157F, 0.9749462029F, 0.9750398435F, 0.9751332378F,
  0.9752263861F, 0.9753192887F, 0.9754119461F, 0.9755043585F,
  0.9755965262F, 0.9756884496F, 0.9757801291F, 0.9758715650F,
  0.9759627575F, 0.9760537071F, 0.9761444141F, 0.9762348789F,
  0.9763251016F, 0.9764150828F, 0.9765048228F, 0.9765943218F,
  0.9766835802F, 0.9767725984F, 0.9768613767F, 0.9769499154F,
  0.9770382149F, 0.9771262755F, 0.9772140976F, 0.9773016815F,
  0.9773890275F, 0.9774761360F, 0.9775630073F, 0.9776496418F,
  0.9777360398F, 0.9778222016F, 0.9779081277F, 0.9779938182F,
  0.9780792736F, 0.9781644943F, 0.9782494805F, 0.9783342326F,
  0.9784187509F, 0.9785030359F, 0.9785870877F, 0.9786709069F,
  0.9787544936F, 0.9788378484F, 0.9789209714F, 0.9790038631F,
  0.9790865238F, 0.9791689538F, 0.9792511535F, 0.9793331232F,
  0.9794148633F, 0.9794963742F, 0.9795776561F, 0.9796587094F,
  0.9797395345F, 0.9798201316F, 0.9799005013F, 0.9799806437F,
  0.9800605593F, 0.9801402483F, 0.9802197112F, 0.9802989483F,
  0.9803779600F, 0.9804567465F, 0.9805353082F, 0.9806136455F,
  0.9806917587F, 0.9807696482F, 0.9808473143F, 0.9809247574F,
  0.9810019778F, 0.9810789759F, 0.9811557519F, 0.9812323064F,
  0.9813086395F, 0.9813847517F, 0.9814606433F, 0.9815363147F,
  0.9816117662F, 0.9816869981F, 0.9817620108F, 0.9818368047F,
  0.9819113801F, 0.9819857374F, 0.9820598769F, 0.9821337989F,
  0.9822075038F, 0.9822809920F, 0.9823542638F, 0.9824273195F,
  0.9825001596F, 0.9825727843F, 0.9826451940F, 0.9827173891F,
  0.9827893700F, 0.9828611368F, 0.9829326901F, 0.9830040302F,
  0.9830751574F, 0.9831460720F, 0.9832167745F, 0.9832872652F,
  0.9833575444F, 0.9834276124F, 0.9834974697F, 0.9835671166F,
  0.9836365535F, 0.9837057806F, 0.9837747983F, 0.9838436071F,
  0.9839122072F, 0.9839805990F, 0.9840487829F, 0.9841167591F,
  0.9841845282F, 0.9842520903F, 0.9843194459F, 0.9843865953F,
  0.9844535389F, 0.9845202771F, 0.9845868101F, 0.9846531383F,
  0.9847192622F, 0.9847851820F, 0.9848508980F, 0.9849164108F,
  0.9849817205F, 0.9850468276F, 0.9851117324F, 0.9851764352F,
  0.9852409365F, 0.9853052366F, 0.9853693358F, 0.9854332344F,
  0.9854969330F, 0.9855604317F, 0.9856237309F, 0.9856868310F,
  0.9857497325F, 0.9858124355F, 0.9858749404F, 0.9859372477F,
  0.9859993577F, 0.9860612707F, 0.9861229871F, 0.9861845072F,
  0.9862458315F, 0.9863069601F, 0.9863678936F, 0.9864286322F,
  0.9864891764F, 0.9865495264F, 0.9866096826F, 0.9866696454F,
  0.9867294152F, 0.9867889922F, 0.9868483769F, 0.9869075695F,
  0.9869665706F, 0.9870253803F, 0.9870839991F, 0.9871424273F,
  0.9872006653F, 0.9872587135F, 0.9873165721F, 0.9873742415F,
  0.9874317222F, 0.9874890144F, 0.9875461185F, 0.9876030348F,
  0.9876597638F, 0.9877163057F, 0.9877726610F, 0.9878288300F,
  0.9878848130F, 0.9879406104F, 0.9879962225F, 0.9880516497F,
  0.9881068924F, 0.9881619509F, 0.9882168256F, 0.9882715168F,
  0.9883260249F, 0.9883803502F, 0.9884344931F, 0.9884884539F,
  0.9885422331F, 0.9885958309F, 0.9886492477F, 0.9887024838F,
  0.9887555397F, 0.9888084157F, 0.9888611120F, 0.9889136292F,
  0.9889659675F, 0.9890181273F, 0.9890701089F, 0.9891219128F,
  0.9891735392F, 0.9892249885F, 0.9892762610F, 0.9893273572F,
  0.9893782774F, 0.9894290219F, 0.9894795911F, 0.9895299853F,
  0.9895802049F, 0.9896302502F, 0.9896801217F, 0.9897298196F,
  0.9897793443F, 0.9898286961F, 0.9898778755F, 0.9899268828F,
  0.9899757183F, 0.9900243823F, 0.9900728753F, 0.9901211976F,
  0.9901693495F, 0.9902173314F, 0.9902651436F, 0.9903127865F,
  0.9903602605F, 0.9904075659F, 0.9904547031F, 0.9905016723F,
  0.9905484740F, 0.9905951086F, 0.9906415763F, 0.9906878775F,
  0.9907340126F, 0.9907799819F, 0.9908257858F, 0.9908714247F,
  0.9909168988F, 0.9909622086F, 0.9910073543F, 0.9910523364F,
  0.9910971552F, 0.9911418110F, 0.9911863042F, 0.9912306351F,
  0.9912748042F, 0.9913188117F, 0.9913626580F, 0.9914063435F,
  0.9914498684F, 0.9914932333F, 0.9915364383F, 0.9915794839F,
  0.9916223703F, 0.9916650981F, 0.9917076674F, 0.9917500787F,
  0.9917923323F, 0.9918344286F, 0.9918763679F, 0.9919181505F,
  0.9919597769F, 0.9920012473F, 0.9920425621F, 0.9920837217F,
  0.9921247263F, 0.9921655765F, 0.9922062724F, 0.9922468145F,
  0.9922872030F, 0.9923274385F, 0.9923675211F, 0.9924074513F,
  0.9924472294F, 0.9924868557F, 0.9925263306F, 0.9925656544F,
  0.9926048275F, 0.9926438503F, 0.9926827230F, 0.9927214461F,
  0.9927600199F, 0.9927984446F, 0.9928367208F, 0.9928748486F,
  0.9929128285F, 0.9929506608F, 0.9929883459F, 0.9930258841F,
  0.9930632757F, 0.9931005211F, 0.9931376207F, 0.9931745747F,
  0.9932113836F, 0.9932480476F, 0.9932845671F, 0.9933209425F,
  0.9933571742F, 0.9933932623F, 0.9934292074F, 0.9934650097F,
  0.9935006696F, 0.9935361874F, 0.9935715635F, 0.9936067982F,
  0.9936418919F, 0.9936768448F, 0.9937116574F, 0.9937463300F,
  0.9937808629F, 0.9938152565F, 0.9938495111F, 0.9938836271F,
  0.9939176047F, 0.9939514444F, 0.9939851465F, 0.9940187112F,
  0.9940521391F, 0.9940854303F, 0.9941185853F, 0.9941516044F,
  0.9941844879F, 0.9942172361F, 0.9942498495F, 0.9942823283F,
  0.9943146729F, 0.9943468836F, 0.9943789608F, 0.9944109047F,
  0.9944427158F, 0.9944743944F, 0.9945059408F, 0.9945373553F,
  0.9945686384F, 0.9945997902F, 0.9946308112F, 0.9946617017F,
  0.9946924621F, 0.9947230926F, 0.9947535937F, 0.9947839656F,
  0.9948142086F, 0.9948443232F, 0.9948743097F, 0.9949041683F,
  0.9949338995F, 0.9949635035F, 0.9949929807F, 0.9950223315F,
  0.9950515561F, 0.9950806549F, 0.9951096282F, 0.9951384764F,
  0.9951671998F, 0.9951957987F, 0.9952242735F, 0.9952526245F,
  0.9952808520F, 0.9953089564F, 0.9953369380F, 0.9953647971F,
  0.9953925340F, 0.9954201491F, 0.9954476428F, 0.9954750153F,
  0.9955022670F, 0.9955293981F, 0.9955564092F, 0.9955833003F,
  0.9956100720F, 0.9956367245F, 0.9956632582F, 0.9956896733F,
  0.9957159703F, 0.9957421494F, 0.9957682110F, 0.9957941553F,
  0.9958199828F, 0.9958456937F, 0.9958712884F, 0.9958967672F,
  0.9959221305F, 0.9959473784F, 0.9959725115F, 0.9959975300F,
  0.9960224342F, 0.9960472244F, 0.9960719011F, 0.9960964644F,
  0.9961209148F, 0.9961452525F, 0.9961694779F, 0.9961935913F,
  0.9962175930F, 0.9962414834F, 0.9962652627F, 0.9962889313F,
  0.9963124895F, 0.9963359377F, 0.9963592761F, 0.9963825051F,
  0.9964056250F, 0.9964286361F, 0.9964515387F, 0.9964743332F,
  0.9964970198F, 0.9965195990F, 0.9965420709F, 0.9965644360F,
  0.9965866946F, 0.9966088469F, 0.9966308932F, 0.9966528340F,
  0.9966746695F, 0.9966964001F, 0.9967180260F, 0.9967395475F,
  0.9967609651F, 0.9967822789F, 0.9968034894F, 0.9968245968F,
  0.9968456014F, 0.9968665036F, 0.9968873037F, 0.9969080019F,
  0.9969285987F, 0.9969490942F, 0.9969694889F, 0.9969897830F,
  0.9970099769F, 0.9970300708F, 0.9970500651F, 0.9970699601F,
  0.9970897561F, 0.9971094533F, 0.9971290522F, 0.9971485531F,
  0.9971679561F, 0.9971872617F, 0.9972064702F, 0.9972255818F,
  0.9972445968F, 0.9972635157F, 0.9972823386F, 0.9973010659F,
  0.9973196980F, 0.9973382350F, 0.9973566773F, 0.9973750253F,
  0.9973932791F, 0.9974114392F, 0.9974295059F, 0.9974474793F,
  0.9974653599F, 0.9974831480F, 0.9975008438F, 0.9975184476F,
  0.9975359598F, 0.9975533806F, 0.9975707104F, 0.9975879495F,
  0.9976050981F, 0.9976221566F, 0.9976391252F, 0.9976560043F,
  0.9976727941F, 0.9976894950F, 0.9977061073F, 0.9977226312F,
  0.9977390671F, 0.9977554152F, 0.9977716759F, 0.9977878495F,
  0.9978039361F, 0.9978199363F, 0.9978358501F, 0.9978516780F,
  0.9978674202F, 0.9978830771F, 0.9978986488F, 0.9979141358F,
  0.9979295383F, 0.9979448566F, 0.9979600909F, 0.9979752417F,
  0.9979903091F, 0.9980052936F, 0.9980201952F, 0.9980350145F,
  0.9980497515F, 0.9980644067F, 0.9980789804F, 0.9980934727F,
  0.9981078841F, 0.9981222147F, 0.9981364649F, 0.9981506350F,
  0.9981647253F, 0.9981787360F, 0.9981926674F, 0.9982065199F,
  0.9982202936F, 0.9982339890F, 0.9982476062F, 0.9982611456F,
  0.9982746074F, 0.9982879920F, 0.9983012996F, 0.9983145304F,
  0.9983276849F, 0.9983407632F, 0.9983537657F, 0.9983666926F,
  0.9983795442F, 0.9983923208F, 0.9984050226F, 0.9984176501F,
  0.9984302033F, 0.9984426827F, 0.9984550884F, 0.9984674208F,
  0.9984796802F, 0.9984918667F, 0.9985039808F, 0.9985160227F,
  0.9985279926F, 0.9985398909F, 0.9985517177F, 0.9985634734F,
  0.9985751583F, 0.9985867727F, 0.9985983167F, 0.9986097907F,
  0.9986211949F, 0.9986325297F, 0.9986437953F, 0.9986549919F,
  0.9986661199F, 0.9986771795F, 0.9986881710F, 0.9986990946F,
  0.9987099507F, 0.9987207394F, 0.9987314611F, 0.9987421161F,
  0.9987527045F, 0.9987632267F, 0.9987736829F, 0.9987840734F,
  0.9987943985F, 0.9988046584F, 0.9988148534F, 0.9988249838F,
  0.9988350498F, 0.9988450516F, 0.9988549897F, 0.9988648641F,
  0.9988746753F, 0.9988844233F, 0.9988941086F, 0.9989037313F,
  0.9989132918F, 0.9989227902F, 0.9989322269F, 0.9989416021F,
  0.9989509160F, 0.9989601690F, 0.9989693613F, 0.9989784931F,
  0.9989875647F, 0.9989965763F, 0.9990055283F, 0.9990144208F,
  0.9990232541F, 0.9990320286F, 0.9990407443F, 0.9990494016F,
  0.9990580008F, 0.9990665421F, 0.9990750257F, 0.9990834519F,
  0.9990918209F, 0.9991001331F, 0.9991083886F, 0.9991165877F,
  0.9991247307F, 0.9991328177F, 0.9991408491F, 0.9991488251F,
  0.9991567460F, 0.9991646119F, 0.9991724232F, 0.9991801801F,
  0.9991878828F, 0.9991955316F, 0.9992031267F, 0.9992106684F,
  0.9992181569F, 0.9992255925F, 0.9992329753F, 0.9992403057F,
  0.9992475839F, 0.9992548101F, 0.9992619846F, 0.9992691076F,
  0.9992761793F, 0.9992832001F, 0.9992901701F, 0.9992970895F,
  0.9993039587F, 0.9993107777F, 0.9993175470F, 0.9993242667F,
  0.9993309371F, 0.9993375583F, 0.9993441307F, 0.9993506545F,
  0.9993571298F, 0.9993635570F, 0.9993699362F, 0.9993762678F,
  0.9993825519F, 0.9993887887F, 0.9993949785F, 0.9994011216F,
  0.9994072181F, 0.9994132683F, 0.9994192725F, 0.9994252307F,
  0.9994311434F, 0.9994370107F, 0.9994428327F, 0.9994486099F,
  0.9994543423F, 0.9994600303F, 0.9994656739F, 0.9994712736F,
  0.9994768294F, 0.9994823417F, 0.9994878105F, 0.9994932363F,
  0.9994986191F, 0.9995039592F, 0.9995092568F, 0.9995145122F,
  0.9995197256F, 0.9995248971F, 0.9995300270F, 0.9995351156F,
  0.9995401630F, 0.9995451695F, 0.9995501352F, 0.9995550604F,
  0.9995599454F, 0.9995647903F, 0.9995695953F, 0.9995743607F,
  0.9995790866F, 0.9995837734F, 0.9995884211F, 0.9995930300F,
  0.9995976004F, 0.9996021324F, 0.9996066263F, 0.9996110822F,
  0.9996155004F, 0.9996198810F, 0.9996242244F, 0.9996285306F,
  0.9996327999F, 0.9996370326F, 0.9996412287F, 0.9996453886F,
  0.9996495125F, 0.9996536004F, 0.9996576527F, 0.9996616696F,
  0.9996656512F, 0.9996695977F, 0.9996735094F, 0.9996773865F,
  0.9996812291F, 0.9996850374F, 0.9996888118F, 0.9996925523F,
  0.9996962591F, 0.9996999325F, 0.9997035727F, 0.9997071798F,
  0.9997107541F, 0.9997142957F, 0.9997178049F, 0.9997212818F,
  0.9997247266F, 0.9997281396F, 0.9997315209F, 0.9997348708F,
  0.9997381893F, 0.9997414767F, 0.9997447333F, 0.9997479591F,
  0.9997511544F, 0.9997543194F, 0.9997574542F, 0.9997605591F,
  0.9997636342F, 0.9997666797F, 0.9997696958F, 0.9997726828F,
  0.9997756407F, 0.9997785698F, 0.9997814703F, 0.9997843423F,
  0.9997871860F, 0.9997900016F, 0.9997927894F, 0.9997955494F,
  0.9997982818F, 0.9998009869F, 0.9998036648F, 0.9998063157F,
  0.9998089398F, 0.9998115373F, 0.9998141082F, 0.9998166529F,
  0.9998191715F, 0.9998216642F, 0.9998241311F, 0.9998265724F,
  0.9998289884F, 0.9998313790F, 0.9998337447F, 0.9998360854F,
  0.9998384015F, 0.9998406930F, 0.9998429602F, 0.9998452031F,
  0.9998474221F, 0.9998496171F, 0.9998517885F, 0.9998539364F,
  0.9998560610F, 0.9998581624F, 0.9998602407F, 0.9998622962F,
  0.9998643291F, 0.9998663394F, 0.9998683274F, 0.9998702932F,
  0.9998722370F, 0.9998741589F, 0.9998760591F, 0.9998779378F,
  0.9998797952F, 0.9998816313F, 0.9998834464F, 0.9998852406F,
  0.9998870141F, 0.9998887670F, 0.9998904995F, 0.9998922117F,
  0.9998939039F, 0.9998955761F, 0.9998972285F, 0.9998988613F,
  0.9999004746F, 0.9999020686F, 0.9999036434F, 0.9999051992F,
  0.9999067362F, 0.9999082544F, 0.9999097541F, 0.9999112354F,
  0.9999126984F, 0.9999141433F, 0.9999155703F, 0.9999169794F,
  0.9999183709F, 0.9999197449F, 0.9999211014F, 0.9999224408F,
  0.9999237631F, 0.9999250684F, 0.9999263570F, 0.9999276289F,
  0.9999288843F, 0.9999301233F, 0.9999313461F, 0.9999325529F,
  0.9999337437F, 0.9999349187F, 0.9999360780F, 0.9999372218F,
  0.9999383503F, 0.9999394635F, 0.9999405616F, 0.9999416447F,
  0.9999427129F, 0.9999437665F, 0.9999448055F, 0.9999458301F,
  0.9999468404F, 0.9999478365F, 0.9999488185F, 0.9999497867F,
  0.9999507411F, 0.9999516819F, 0.9999526091F, 0.9999535230F,
  0.9999544236F, 0.9999553111F, 0.9999561856F, 0.9999570472F,
  0.9999578960F, 0.9999587323F, 0.9999595560F, 0.9999603674F,
  0.9999611666F, 0.9999619536F, 0.9999627286F, 0.9999634917F,
  0.9999642431F, 0.9999649828F, 0.9999657110F, 0.9999664278F,
  0.9999671334F, 0.9999678278F, 0.9999685111F, 0.9999691835F,
  0.9999698451F, 0.9999704960F, 0.9999711364F, 0.9999717662F,
  0.9999723858F, 0.9999729950F, 0.9999735942F, 0.9999741834F,
  0.9999747626F, 0.9999753321F, 0.9999758919F, 0.9999764421F,
  0.9999769828F, 0.9999775143F, 0.9999780364F, 0.9999785495F,
  0.9999790535F, 0.9999795485F, 0.9999800348F, 0.9999805124F,
  0.9999809813F, 0.9999814417F, 0.9999818938F, 0.9999823375F,
  0.9999827731F, 0.9999832005F, 0.9999836200F, 0.9999840316F,
  0.9999844353F, 0.9999848314F, 0.9999852199F, 0.9999856008F,
  0.9999859744F, 0.9999863407F, 0.9999866997F, 0.9999870516F,
  0.9999873965F, 0.9999877345F, 0.9999880656F, 0.9999883900F,
  0.9999887078F, 0.9999890190F, 0.9999893237F, 0.9999896220F,
  0.9999899140F, 0.9999901999F, 0.9999904796F, 0.9999907533F,
  0.9999910211F, 0.9999912830F, 0.9999915391F, 0.9999917896F,
  0.9999920345F, 0.9999922738F, 0.9999925077F, 0.9999927363F,
  0.9999929596F, 0.9999931777F, 0.9999933907F, 0.9999935987F,
  0.9999938018F, 0.9999940000F, 0.9999941934F, 0.9999943820F,
  0.9999945661F, 0.9999947456F, 0.9999949206F, 0.9999950912F,
  0.9999952575F, 0.9999954195F, 0.9999955773F, 0.9999957311F,
  0.9999958807F, 0.9999960265F, 0.9999961683F, 0.9999963063F,
  0.9999964405F, 0.9999965710F, 0.9999966979F, 0.9999968213F,
  0.9999969412F, 0.9999970576F, 0.9999971707F, 0.9999972805F,
  0.9999973871F, 0.9999974905F, 0.9999975909F, 0.9999976881F,
  0.9999977824F, 0.9999978738F, 0.9999979624F, 0.9999980481F,
  0.9999981311F, 0.9999982115F, 0.9999982892F, 0.9999983644F,
  0.9999984370F, 0.9999985072F, 0.9999985750F, 0.9999986405F,
  0.9999987037F, 0.9999987647F, 0.9999988235F, 0.9999988802F,
  0.9999989348F, 0.9999989873F, 0.9999990379F, 0.9999990866F,
  0.9999991334F, 0.9999991784F, 0.9999992217F, 0.9999992632F,
  0.9999993030F, 0.9999993411F, 0.9999993777F, 0.9999994128F,
  0.9999994463F, 0.9999994784F, 0.9999995091F, 0.9999995384F,
  0.9999995663F, 0.9999995930F, 0.9999996184F, 0.9999996426F,
  0.9999996657F, 0.9999996876F, 0.9999997084F, 0.9999997282F,
  0.9999997469F, 0.9999997647F, 0.9999997815F, 0.9999997973F,
  0.9999998123F, 0.9999998265F, 0.9999998398F, 0.9999998524F,
  0.9999998642F, 0.9999998753F, 0.9999998857F, 0.9999998954F,
  0.9999999045F, 0.9999999130F, 0.9999999209F, 0.9999999282F,
  0.9999999351F, 0.9999999414F, 0.9999999472F, 0.9999999526F,
  0.9999999576F, 0.9999999622F, 0.9999999664F, 0.9999999702F,
  0.9999999737F, 0.9999999769F, 0.9999999798F, 0.9999999824F,
  0.9999999847F, 0.9999999868F, 0.9999999887F, 0.9999999904F,
  0.9999999919F, 0.9999999932F, 0.9999999943F, 0.9999999953F,
  0.9999999961F, 0.9999999969F, 0.9999999975F, 0.9999999980F,
  0.9999999985F, 0.9999999988F, 0.9999999991F, 0.9999999993F,
  0.9999999995F, 0.9999999997F, 0.9999999998F, 0.9999999999F,
  0.9999999999F, 1.0000000000F, 1.0000000000F, 1.0000000000F,
  1.0000000000F, 1.0000000000F, 1.0000000000F, 1.0000000000F,
};

static const float *const vwin[8] = {
  vwin64,
  vwin128,
  vwin256,
  vwin512,
  vwin1024,
  vwin2048,
  vwin4096,
  vwin8192,
};

const float *_vorbis_window_get(int n){
  return vwin[n];
}

void _vorbis_apply_window(float *d,int *winno,long *blocksizes,
                          int lW,int W,int nW){
  lW=(W?lW:0);
  nW=(W?nW:0);

  {
    const float *windowLW=vwin[winno[lW]];
    const float *windowNW=vwin[winno[nW]];

    long n=blocksizes[W];
    long ln=blocksizes[lW];
    long rn=blocksizes[nW];

    long leftbegin=n/4-ln/4;
    long leftend=leftbegin+ln/2;

    long rightbegin=n/2+n/4-rn/4;
    long rightend=rightbegin+rn/2;

    int i,p;

    for(i=0;i<leftbegin;i++)
      d[i]=0.f;

    for(p=0;i<leftend;i++,p++)
      d[i]*=windowLW[p];

    for(i=rightbegin,p=rn/2-1;i<rightend;i++,p--)
      d[i]*=windowNW[p];

    for(;i<n;i++)
      d[i]=0.f;
  }
}
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

  function: LSP (also called LSF) conversion routines

  The LSP generation code is taken (with minimal modification and a
  few bugfixes) from "On the Computation of the LSP Frequencies" by
  Joseph Rothweiler (see http://www.rothweiler.us for contact info).

  The paper is available at:

  https://web.archive.org/web/20110810174000/http://home.myfairpoint.net/vzenxj75/myown1/joe/lsf/index.html

 ********************************************************************/

/* Note that the lpc-lsp conversion finds the roots of polynomial with
   an iterative root polisher (CACM algorithm 283).  It *is* possible
   to confuse this algorithm into not converging; that should only
   happen with absurdly closely spaced roots (very sharp peaks in the
   LPC f response) which in turn should be impossible in our use of
   the code.  If this *does* happen anyway, it's a bug in the floor
   finder; find the cause of the confusion (probably a single bin
   spike or accidental near-float-limit resolution problems) and
   correct it. */

#include <math.h>
#include <string.h>
#include <stdlib.h>
/*#include "lsp.h"*/
/*#include "os.h"*/
/*#include "misc.h"*/
/*#include "lookup.h"*/
/*#include "scales.h"*/

/* three possible LSP to f curve functions; the exact computation
   (float), a lookup based float implementation, and an integer
   implementation.  The float lookup is likely the optimal choice on
   any machine with an FPU.  The integer implementation is *not* fixed
   point (due to the need for a large dynamic range and thus a
   separately tracked exponent) and thus much more complex than the
   relatively simple float implementations. It's mostly for future
   work on a fully fixed point implementation for processors like the
   ARM family. */

/* define either of these (preferably FLOAT_LOOKUP) to have faster
   but less precise implementation. */
#undef FLOAT_LOOKUP
#undef INT_LOOKUP

#ifdef FLOAT_LOOKUP
/*#include "lookup.c"*/ /* catch this in the build system; we #include for
                       compilers (like gcc) that can't inline across
                       modules */

/* side effect: changes *lsp to cosines of lsp */
void vorbis_lsp_to_curve(float *curve,int *map,int n,int ln,float *lsp,int m,
                            float amp,float ampoffset){
  int i;
  float wdel=M_PI/ln;
  vorbis_fpu_control fpu;

  vorbis_fpu_setround(&fpu);
  for(i=0;i<m;i++)lsp[i]=vorbis_coslook(lsp[i]);

  i=0;
  while(i<n){
    int k=map[i];
    int qexp;
    float p=.7071067812f;
    float q=.7071067812f;
    float w=vorbis_coslook(wdel*k);
    float *ftmp=lsp;
    int c=m>>1;

    while(c--){
      q*=ftmp[0]-w;
      p*=ftmp[1]-w;
      ftmp+=2;
    }

    if(m&1){
      /* odd order filter; slightly assymetric */
      /* the last coefficient */
      q*=ftmp[0]-w;
      q*=q;
      p*=p*(1.f-w*w);
    }else{
      /* even order filter; still symmetric */
      q*=q*(1.f+w);
      p*=p*(1.f-w);
    }

    q=frexp(p+q,&qexp);
    q=vorbis_fromdBlook(amp*
                        vorbis_invsqlook(q)*
                        vorbis_invsq2explook(qexp+m)-
                        ampoffset);

    do{
      curve[i++]*=q;
    }while(map[i]==k);
  }
  vorbis_fpu_restore(fpu);
}

#else

#ifdef INT_LOOKUP
/*#include "lookup.c"*/ /* catch this in the build system; we #include for
                       compilers (like gcc) that can't inline across
                       modules */

static const int MLOOP_1[64]={
   0,10,11,11, 12,12,12,12, 13,13,13,13, 13,13,13,13,
  14,14,14,14, 14,14,14,14, 14,14,14,14, 14,14,14,14,
  15,15,15,15, 15,15,15,15, 15,15,15,15, 15,15,15,15,
  15,15,15,15, 15,15,15,15, 15,15,15,15, 15,15,15,15,
};

static const int MLOOP_2[64]={
  0,4,5,5, 6,6,6,6, 7,7,7,7, 7,7,7,7,
  8,8,8,8, 8,8,8,8, 8,8,8,8, 8,8,8,8,
  9,9,9,9, 9,9,9,9, 9,9,9,9, 9,9,9,9,
  9,9,9,9, 9,9,9,9, 9,9,9,9, 9,9,9,9,
};

static const int MLOOP_3[8]={0,1,2,2,3,3,3,3};


/* side effect: changes *lsp to cosines of lsp */
void vorbis_lsp_to_curve(float *curve,int *map,int n,int ln,float *lsp,int m,
                            float amp,float ampoffset){

  /* 0 <= m < 256 */

  /* set up for using all int later */
  int i;
  int ampoffseti=rint(ampoffset*4096.f);
  int ampi=rint(amp*16.f);
  long *ilsp=alloca(m*sizeof(*ilsp));
  for(i=0;i<m;i++)ilsp[i]=vorbis_coslook_i(lsp[i]/M_PI*65536.f+.5f);

  i=0;
  while(i<n){
    int j,k=map[i];
    unsigned long pi=46341; /* 2**-.5 in 0.16 */
    unsigned long qi=46341;
    int qexp=0,shift;
    long wi=vorbis_coslook_i(k*65536/ln);

    qi*=labs(ilsp[0]-wi);
    pi*=labs(ilsp[1]-wi);

    for(j=3;j<m;j+=2){
      if(!(shift=MLOOP_1[(pi|qi)>>25]))
        if(!(shift=MLOOP_2[(pi|qi)>>19]))
          shift=MLOOP_3[(pi|qi)>>16];
      qi=(qi>>shift)*labs(ilsp[j-1]-wi);
      pi=(pi>>shift)*labs(ilsp[j]-wi);
      qexp+=shift;
    }
    if(!(shift=MLOOP_1[(pi|qi)>>25]))
      if(!(shift=MLOOP_2[(pi|qi)>>19]))
        shift=MLOOP_3[(pi|qi)>>16];

    /* pi,qi normalized collectively, both tracked using qexp */

    if(m&1){
      /* odd order filter; slightly assymetric */
      /* the last coefficient */
      qi=(qi>>shift)*labs(ilsp[j-1]-wi);
      pi=(pi>>shift)<<14;
      qexp+=shift;

      if(!(shift=MLOOP_1[(pi|qi)>>25]))
        if(!(shift=MLOOP_2[(pi|qi)>>19]))
          shift=MLOOP_3[(pi|qi)>>16];

      pi>>=shift;
      qi>>=shift;
      qexp+=shift-14*((m+1)>>1);

      pi=((pi*pi)>>16);
      qi=((qi*qi)>>16);
      qexp=qexp*2+m;

      pi*=(1<<14)-((wi*wi)>>14);
      qi+=pi>>14;

    }else{
      /* even order filter; still symmetric */

      /* p*=p(1-w), q*=q(1+w), let normalization drift because it isn't
         worth tracking step by step */

      pi>>=shift;
      qi>>=shift;
      qexp+=shift-7*m;

      pi=((pi*pi)>>16);
      qi=((qi*qi)>>16);
      qexp=qexp*2+m;

      pi*=(1<<14)-wi;
      qi*=(1<<14)+wi;
      qi=(qi+pi)>>14;

    }


    /* we've let the normalization drift because it wasn't important;
       however, for the lookup, things must be normalized again.  We
       need at most one right shift or a number of left shifts */

    if(qi&0xffff0000){ /* checks for 1.xxxxxxxxxxxxxxxx */
      qi>>=1; qexp++;
    }else
      while(qi && !(qi&0x8000)){ /* checks for 0.0xxxxxxxxxxxxxxx or less*/
        qi<<=1; qexp--;
      }

    amp=vorbis_fromdBlook_i(ampi*                     /*  n.4         */
                            vorbis_invsqlook_i(qi,qexp)-
                                                      /*  m.8, m+n<=8 */
                            ampoffseti);              /*  8.12[0]     */

    curve[i]*=amp;
    while(map[++i]==k)curve[i]*=amp;
  }
}

#else

/* old, nonoptimized but simple version for any poor sap who needs to
   figure out what the hell this code does, or wants the other
   fraction of a dB precision */

/* side effect: changes *lsp to cosines of lsp */
void vorbis_lsp_to_curve(float *curve,int *map,int n,int ln,float *lsp,int m,
                            float amp,float ampoffset){
  int i;
  float wdel=M_PI/ln;
  for(i=0;i<m;i++)lsp[i]=2.f*cos(lsp[i]);

  i=0;
  while(i<n){
    int j,k=map[i];
    float p=.5f;
    float q=.5f;
    float w=2.f*cos(wdel*k);
    for(j=1;j<m;j+=2){
      q *= w-lsp[j-1];
      p *= w-lsp[j];
    }
    if(j==m){
      /* odd order filter; slightly assymetric */
      /* the last coefficient */
      q*=w-lsp[j-1];
      p*=p*(4.f-w*w);
      q*=q;
    }else{
      /* even order filter; still symmetric */
      p*=p*(2.f-w);
      q*=q*(2.f+w);
    }

    q=fromdB(amp/sqrt(p+q)-ampoffset);

    curve[i]*=q;
    while(map[++i]==k)curve[i]*=q;
  }
}

#endif
#endif

static void cheby(float *g, int ord) {
  int i, j;

  g[0] *= .5f;
  for(i=2; i<= ord; i++) {
    for(j=ord; j >= i; j--) {
      g[j-2] -= g[j];
      g[j] += g[j];
    }
  }
}

static int comp(const void *a,const void *b){
  return (*(float *)a<*(float *)b)-(*(float *)a>*(float *)b);
}

/* Newton-Raphson-Maehly actually functioned as a decent root finder,
   but there are root sets for which it gets into limit cycles
   (exacerbated by zero suppression) and fails.  We can't afford to
   fail, even if the failure is 1 in 100,000,000, so we now use
   Laguerre and later polish with Newton-Raphson (which can then
   afford to fail) */

#define EPSILON 10e-7
static int Laguerre_With_Deflation(float *a,int ord,float *r){
  int i,m;
  double *defl=(double*)alloca(sizeof(*defl)*(ord+1));
  for(i=0;i<=ord;i++)defl[i]=a[i];

  for(m=ord;m>0;m--){
    double new_=0.f,delta;

    /* iterate a root */
    while(1){
      double p=defl[m],pp=0.f,ppp=0.f,denom;

      /* eval the polynomial and its first two derivatives */
      for(i=m;i>0;i--){
        ppp = new_*ppp + pp;
        pp  = new_*pp  + p;
        p   = new_*p   + defl[i-1];
      }

      /* Laguerre's method */
      denom=(m-1) * ((m-1)*pp*pp - m*p*ppp);
      if(denom<0)
        return(-1);  /* complex root!  The LPC generator handed us a bad filter */

      if(pp>0){
        denom = pp + sqrt(denom);
        if(denom<EPSILON)denom=EPSILON;
      }else{
        denom = pp - sqrt(denom);
        if(denom>-(EPSILON))denom=-(EPSILON);
      }

      delta  = m*p/denom;
      new_   -= delta;

      if(delta<0.f)delta*=-1;

      if(fabs(delta/new_)<10e-12)break;
    }

    r[m-1]=new_;

    /* forward deflation */

    for(i=m;i>0;i--)
      defl[i-1]+=new_*defl[i];
    defl++;

  }
  return(0);
}


/* for spit-and-polish only */
static int Newton_Raphson(float *a,int ord,float *r){
  int i, k, count=0;
  double error=1.f;
  double *root=(double*)alloca(ord*sizeof(*root));

  for(i=0; i<ord;i++) root[i] = r[i];

  while(error>1e-20){
    error=0;

    for(i=0; i<ord; i++) { /* Update each point. */
      double pp=0.,delta;
      double rooti=root[i];
      double p=a[ord];
      for(k=ord-1; k>= 0; k--) {

        pp= pp* rooti + p;
        p = p * rooti + a[k];
      }

      delta = p/pp;
      root[i] -= delta;
      error+= delta*delta;
    }

    if(count>40)return(-1);

    count++;
  }

  /* Replaced the original bubble sort with a real sort.  With your
     help, we can eliminate the bubble sort in our lifetime. --Monty */

  for(i=0; i<ord;i++) r[i] = root[i];
  return(0);
}


/* Convert lpc coefficients to lsp coefficients */
int vorbis_lpc_to_lsp(float *lpc,float *lsp,int m){
  int order2=(m+1)>>1;
  int g1_order,g2_order;
  float *g1=(float*)alloca(sizeof(*g1)*(order2+1));
  float *g2=(float*)alloca(sizeof(*g2)*(order2+1));
  float *g1r=(float*)alloca(sizeof(*g1r)*(order2+1));
  float *g2r=(float*)alloca(sizeof(*g2r)*(order2+1));
  int i;

  /* even and odd are slightly different base cases */
  g1_order=(m+1)>>1;
  g2_order=(m)  >>1;

  /* Compute the lengths of the x polynomials. */
  /* Compute the first half of K & R F1 & F2 polynomials. */
  /* Compute half of the symmetric and antisymmetric polynomials. */
  /* Remove the roots at +1 and -1. */

  g1[g1_order] = 1.f;
  for(i=1;i<=g1_order;i++) g1[g1_order-i] = lpc[i-1]+lpc[m-i];
  g2[g2_order] = 1.f;
  for(i=1;i<=g2_order;i++) g2[g2_order-i] = lpc[i-1]-lpc[m-i];

  if(g1_order>g2_order){
    for(i=2; i<=g2_order;i++) g2[g2_order-i] += g2[g2_order-i+2];
  }else{
    for(i=1; i<=g1_order;i++) g1[g1_order-i] -= g1[g1_order-i+1];
    for(i=1; i<=g2_order;i++) g2[g2_order-i] += g2[g2_order-i+1];
  }

  /* Convert into polynomials in cos(alpha) */
  cheby(g1,g1_order);
  cheby(g2,g2_order);

  /* Find the roots of the 2 even polynomials.*/
  if(Laguerre_With_Deflation(g1,g1_order,g1r) ||
     Laguerre_With_Deflation(g2,g2_order,g2r))
    return(-1);

  Newton_Raphson(g1,g1_order,g1r); /* if it fails, it leaves g1r alone */
  Newton_Raphson(g2,g2_order,g2r); /* if it fails, it leaves g2r alone */

  qsort(g1r,g1_order,sizeof(*g1r),comp);
  qsort(g2r,g2_order,sizeof(*g2r),comp);

  for(i=0;i<g1_order;i++)
    lsp[i*2] = acos(g1r[i]);

  for(i=0;i<g2_order;i++)
    lsp[i*2+1] = acos(g2r[i]);
  return(0);
}
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

  function: LPC low level routines

 ********************************************************************/

/* Some of these routines (autocorrelator, LPC coefficient estimator)
   are derived from code written by Jutta Degener and Carsten Bormann;
   thus we include their copyright below.  The entirety of this file
   is freely redistributable on the condition that both of these
   copyright notices are preserved without modification.  */

/* Preserved Copyright: *********************************************/

/* Copyright 1992, 1993, 1994 by Jutta Degener and Carsten Bormann,
Technische Universita"t Berlin

Any use of this software is permitted provided that this notice is not
removed and that neither the authors nor the Technische Universita"t
Berlin are deemed to have made any representations as to the
suitability of this software for any purpose nor are held responsible
for any defects of this software. THERE IS ABSOLUTELY NO WARRANTY FOR
THIS SOFTWARE.

As a matter of courtesy, the authors request to be informed about uses
this software has found, about bugs in this software, and about any
improvements that may be of general interest.

Berlin, 28.11.1994
Jutta Degener
Carsten Bormann

*********************************************************************/

#include <stdlib.h>
#include <string.h>
#include <math.h>
/*#include "os.h"*/
/*#include "smallft.h"*/
/*#include "lpc.h"*/
/*#include "scales.h"*/
/*#include "misc.h"*/

/* Autocorrelation LPC coeff generation algorithm invented by
   N. Levinson in 1947, modified by J. Durbin in 1959. */

/* Input : n elements of time doamin data
   Output: m lpc coefficients, excitation energy */

float vorbis_lpc_from_data(float *data,float *lpci,int n,int m){
  double *aut=(double*)alloca(sizeof(*aut)*(m+1));
  double *lpc=(double*)(sizeof(*lpc)*(m));
  double error;
  double epsilon;
  int i,j;

  /* autocorrelation, p+1 lag coefficients */
  j=m+1;
  while(j--){
    double d=0; /* double needed for accumulator depth */
    for(i=j;i<n;i++)d+=(double)data[i]*data[i-j];
    aut[j]=d;
  }

  /* Generate lpc coefficients from autocorr values */

  /* set our noise floor to about -100dB */
  error=aut[0] * (1. + 1e-10);
  epsilon=1e-9*aut[0]+1e-10;

  for(i=0;i<m;i++){
    double r= -aut[i+1];

    if(error<epsilon){
      memset(lpc+i,0,(m-i)*sizeof(*lpc));
      goto done;
    }

    /* Sum up this iteration's reflection coefficient; note that in
       Vorbis we don't save it.  If anyone wants to recycle this code
       and needs reflection coefficients, save the results of 'r' from
       each iteration. */

    for(j=0;j<i;j++)r-=lpc[j]*aut[i-j];
    r/=error;

    /* Update LPC coefficients and total error */

    lpc[i]=r;
    for(j=0;j<i/2;j++){
      double tmp=lpc[j];

      lpc[j]+=r*lpc[i-1-j];
      lpc[i-1-j]+=r*tmp;
    }
    if(i&1)lpc[j]+=lpc[j]*r;

    error*=1.-r*r;

  }

 done:

  /* slightly damp the filter */
  {
    double g = .99;
    double damp = g;
    for(j=0;j<m;j++){
      lpc[j]*=damp;
      damp*=g;
    }
  }

  for(j=0;j<m;j++)lpci[j]=(float)lpc[j];

  /* we need the error value to know how big an impulse to hit the
     filter with later */

  return error;
}

void vorbis_lpc_predict(float *coeff,float *prime,int m,
                     float *data,long n){

  /* in: coeff[0...m-1] LPC coefficients
         prime[0...m-1] initial values (allocated size of n+m-1)
    out: data[0...n-1] data samples */

  long i,j,o,p;
  float y;
  float *work=(float*)alloca(sizeof(*work)*(m+n));

  if(!prime)
    for(i=0;i<m;i++)
      work[i]=0.f;
  else
    for(i=0;i<m;i++)
      work[i]=prime[i];

  for(i=0;i<n;i++){
    y=0;
    o=i;
    p=m;
    for(j=0;j<m;j++)
      y-=work[o++]*coeff[--p];

    data[i]=work[o]=y;
  }
}
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2007             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: single-block PCM analysis mode dispatch

 ********************************************************************/

#include <stdio.h>
#include <string.h>
#include <math.h>
/*#include <ogg/ogg.h>*/
/*#include "vorbis/codec.h"*/
/*#include "codec_internal.h"*/
/*#include "registry.h"*/
/*#include "scales.h"*/
/*#include "os.h"*/
/*#include "misc.h"*/

/* decides between modes, dispatches to the appropriate mapping. */
int vorbis_analysis(vorbis_block *vb, ogg_packet *op){
  int ret,i;
  vorbis_block_internal *vbi=(vorbis_block_internal*)vb->internal;

  vb->glue_bits=0;
  vb->time_bits=0;
  vb->floor_bits=0;
  vb->res_bits=0;

  /* first things first.  Make sure encode is ready */
  for(i=0;i<PACKETBLOBS;i++)
    oggpack_reset(vbi->packetblob[i]);

  /* we only have one mapping type (0), and we let the mapping code
     itself figure out what soft mode to use.  This allows easier
     bitrate management */

  if((ret=_mapping_P[0]->forward(vb)))
    return(ret);

  if(op){
    if(vorbis_bitrate_managed(vb))
      /* The app is using a bitmanaged mode... but not using the
         bitrate management interface. */
      return(OV_EINVAL);

    op->packet=oggpack_get_buffer(&vb->opb);
    op->bytes=oggpack_bytes(&vb->opb);
    op->b_o_s=0;
    op->e_o_s=vb->eofflag;
    op->granulepos=vb->granulepos;
    op->packetno=vb->sequence; /* for sake of completeness */
  }
  return(0);
}

#ifdef ANALYSIS
int analysis_noisy=1;

/* there was no great place to put this.... */
void _analysis_output_always(char *base,int i,float *v,int n,int bark,int dB,ogg_int64_t off){
  int j;
  FILE *of;
  char buffer[80];

  sprintf(buffer,"%s_%d.m",base,i);
  of=fopen(buffer,"w");

  if(!of)perror("failed to open data dump file");

  for(j=0;j<n;j++){
    if(bark){
      float b=toBARK((4000.f*j/n)+.25);
      fprintf(of,"%f ",b);
    }else
      if(off!=0)
        fprintf(of,"%f ",(double)(j+off)/8000.);
      else
        fprintf(of,"%f ",(double)j);

    if(dB){
      float val;
      if(v[j]==0.)
        val=-140.;
      else
        val=todB(v+j);
      fprintf(of,"%f\n",val);
    }else{
      fprintf(of,"%f\n",v[j]);
    }
  }
  fclose(of);
}

void _analysis_output(char *base,int i,float *v,int n,int bark,int dB,
                      ogg_int64_t off){
  if(analysis_noisy)_analysis_output_always(base,i,v,n,bark,dB,off);
}

#endif











/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2015             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: single-block PCM synthesis

 ********************************************************************/

#include <stdio.h>
/*#include <ogg/ogg.h>*/
/*#include "vorbis/codec.h"*/
/*#include "codec_internal.h"*/
/*#include "registry.h"*/
/*#include "misc.h"*/
/*#include "os.h"*/

int vorbis_synthesis(vorbis_block *vb,ogg_packet *op){
  vorbis_dsp_state     *vd= vb ? vb->vd : 0;
  private_state        *b= (private_state*)(vd ? vd->backend_state : 0);
  vorbis_info          *vi= vd ? vd->vi : 0;
  codec_setup_info     *ci= (codec_setup_info*)(vi ? vi->codec_setup : 0);
  oggpack_buffer       *opb=vb ? &vb->opb : 0;
  int                   type,mode,i;

  if (!vd || !b || !vi || !ci || !opb) {
    return OV_EBADPACKET;
  }

  /* first things first.  Make sure decode is ready */
  _vorbis_block_ripcord(vb);
  oggpack_readinit(opb,op->packet,op->bytes);

  /* Check the packet type */
  if(oggpack_read(opb,1)!=0){
    /* Oops.  This is not an audio data packet */
    return(OV_ENOTAUDIO);
  }

  /* read our mode and pre/post windowsize */
  mode=oggpack_read(opb,b->modebits);
  if(mode==-1){
    return(OV_EBADPACKET);
  }

  vb->mode=mode;
  if(!ci->mode_param[mode]){
    return(OV_EBADPACKET);
  }

  vb->W=ci->mode_param[mode]->blockflag;
  if(vb->W){

    /* this doesn;t get mapped through mode selection as it's used
       only for window selection */
    vb->lW=oggpack_read(opb,1);
    vb->nW=oggpack_read(opb,1);
    if(vb->nW==-1){
      return(OV_EBADPACKET);
    }
  }else{
    vb->lW=0;
    vb->nW=0;
  }

  /* more setup */
  vb->granulepos=op->granulepos;
  vb->sequence=op->packetno;
  vb->eofflag=op->e_o_s;

  /* alloc pcm passback storage */
  vb->pcmend=ci->blocksizes[vb->W];
  vb->pcm=(float**)_vorbis_block_alloc(vb,sizeof(*vb->pcm)*vi->channels);
  for(i=0;i<vi->channels;i++)
    vb->pcm[i]=(float*)_vorbis_block_alloc(vb,vb->pcmend*sizeof(*vb->pcm[i]));

  /* unpack_header enforces range checking */
  type=ci->map_type[ci->mode_param[mode]->mapping];

  return(_mapping_P[type]->inverse(vb,ci->map_param[ci->mode_param[mode]->
                                                   mapping]));
}

/* used to track pcm position without actually performing decode.
   Useful for sequential 'fast forward' */
int vorbis_synthesis_trackonly(vorbis_block *vb,ogg_packet *op){
  vorbis_dsp_state     *vd=vb->vd;
  private_state        *b=(private_state*)vd->backend_state;
  vorbis_info          *vi=vd->vi;
  codec_setup_info     *ci=(codec_setup_info*)vi->codec_setup;
  oggpack_buffer       *opb=&vb->opb;
  int                   mode;

  /* first things first.  Make sure decode is ready */
  _vorbis_block_ripcord(vb);
  oggpack_readinit(opb,op->packet,op->bytes);

  /* Check the packet type */
  if(oggpack_read(opb,1)!=0){
    /* Oops.  This is not an audio data packet */
    return(OV_ENOTAUDIO);
  }

  /* read our mode and pre/post windowsize */
  mode=oggpack_read(opb,b->modebits);
  if(mode==-1)return(OV_EBADPACKET);

  vb->mode=mode;
  if(!ci->mode_param[mode]){
    return(OV_EBADPACKET);
  }

  vb->W=ci->mode_param[mode]->blockflag;
  if(vb->W){
    vb->lW=oggpack_read(opb,1);
    vb->nW=oggpack_read(opb,1);
    if(vb->nW==-1)   return(OV_EBADPACKET);
  }else{
    vb->lW=0;
    vb->nW=0;
  }

  /* more setup */
  vb->granulepos=op->granulepos;
  vb->sequence=op->packetno;
  vb->eofflag=op->e_o_s;

  /* no pcm */
  vb->pcmend=0;
  vb->pcm=NULL;

  return(0);
}

long vorbis_packet_blocksize(vorbis_info *vi,ogg_packet *op){
  codec_setup_info     *ci=(codec_setup_info*)vi->codec_setup;
  oggpack_buffer       opb;
  int                  mode;

  if(ci==NULL || ci->modes<=0){
    /* codec setup not properly intialized */
    return(OV_EFAULT);
  }

  oggpack_readinit(&opb,op->packet,op->bytes);

  /* Check the packet type */
  if(oggpack_read(&opb,1)!=0){
    /* Oops.  This is not an audio data packet */
    return(OV_ENOTAUDIO);
  }

  /* read our mode and pre/post windowsize */
  mode=oggpack_read(&opb,ov_ilog(ci->modes-1));
  if(mode==-1 || !ci->mode_param[mode])return(OV_EBADPACKET);
  return(ci->blocksizes[ci->mode_param[mode]->blockflag]);
}

int vorbis_synthesis_halfrate(vorbis_info *vi,int flag){
  /* set / clear half-sample-rate mode */
  codec_setup_info     *ci=(codec_setup_info*)vi->codec_setup;

  /* right now, our MDCT can't handle < 64 sample windows. */
  if(ci->blocksizes[0]<=64 && flag)return -1;
  ci->halfrate_flag=(flag?1:0);
  return 0;
}

int vorbis_synthesis_halfrate_p(vorbis_info *vi){
  codec_setup_info     *ci=(codec_setup_info*)vi->codec_setup;
  return ci->halfrate_flag;
}
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2010             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: psychoacoustics not including preecho

 ********************************************************************/

#include <stdlib.h>
#include <math.h>
#include <string.h>
/*#include "vorbis/codec.h"*/
/*#include "codec_internal.h"*/

/*#include "masking.h"*/
/*#include "psy.h"*/
/*#include "os.h"*/
/*#include "lpc.h"*/
/*#include "smallft.h"*/
/*#include "scales.h"*/
/*#include "misc.h"*/

#define NEGINF -9999.f
static const double stereo_threshholds[]={0.0, .5, 1.0, 1.5, 2.5, 4.5, 8.5, 16.5, 9e10};
static const double stereo_threshholds_limited[]={0.0, .5, 1.0, 1.5, 2.0, 2.5, 4.5, 8.5, 9e10};

vorbis_look_psy_global *_vp_global_look(vorbis_info *vi){
  codec_setup_info *ci=(codec_setup_info*)vi->codec_setup;
  vorbis_info_psy_global *gi=&ci->psy_g_param;
  vorbis_look_psy_global *look=(vorbis_look_psy_global*)_ogg_calloc(1,sizeof(*look));

  look->channels=vi->channels;

  look->ampmax=-9999.;
  look->gi=gi;
  return(look);
}

void _vp_global_free(vorbis_look_psy_global *look){
  if(look){
    memset(look,0,sizeof(*look));
    _ogg_free(look);
  }
}

void _vi_gpsy_free(vorbis_info_psy_global *i){
  if(i){
    memset(i,0,sizeof(*i));
    _ogg_free(i);
  }
}

void _vi_psy_free(vorbis_info_psy *i){
  if(i){
    memset(i,0,sizeof(*i));
    _ogg_free(i);
  }
}

static void min_curve(float *c,
                       float *c2){
  int i;
  for(i=0;i<EHMER_MAX;i++)if(c2[i]<c[i])c[i]=c2[i];
}
static void max_curve(float *c,
                       float *c2){
  int i;
  for(i=0;i<EHMER_MAX;i++)if(c2[i]>c[i])c[i]=c2[i];
}

static void attenuate_curve(float *c,float att){
  int i;
  for(i=0;i<EHMER_MAX;i++)
    c[i]+=att;
}

static float ***setup_tone_curves(float curveatt_dB[P_BANDS],float binHz,int n,
                                  float center_boost, float center_decay_rate){
  int i,j,k,m;
  float ath[EHMER_MAX];
  float workc[P_BANDS][P_LEVELS][EHMER_MAX];
  float athc[P_LEVELS][EHMER_MAX];
  float *brute_buffer=(float*)alloca(n*sizeof(*brute_buffer));

  float ***ret=(float***)_ogg_malloc(sizeof(*ret)*P_BANDS);

  memset(workc,0,sizeof(workc));

  for(i=0;i<P_BANDS;i++){
    /* we add back in the ATH to avoid low level curves falling off to
       -infinity and unnecessarily cutting off high level curves in the
       curve limiting (last step). */

    /* A half-band's settings must be valid over the whole band, and
       it's better to mask too little than too much */
    int ath_offset=i*4;
    for(j=0;j<EHMER_MAX;j++){
      float min=999.;
      for(k=0;k<4;k++)
        if(j+k+ath_offset<MAX_ATH){
          if(min>ATH[j+k+ath_offset])min=ATH[j+k+ath_offset];
        }else{
          if(min>ATH[MAX_ATH-1])min=ATH[MAX_ATH-1];
        }
      ath[j]=min;
    }

    /* copy curves into working space, replicate the 50dB curve to 30
       and 40, replicate the 100dB curve to 110 */
    for(j=0;j<6;j++)
      memcpy(workc[i][j+2],tonemasks[i][j],EHMER_MAX*sizeof(*tonemasks[i][j]));
    memcpy(workc[i][0],tonemasks[i][0],EHMER_MAX*sizeof(*tonemasks[i][0]));
    memcpy(workc[i][1],tonemasks[i][0],EHMER_MAX*sizeof(*tonemasks[i][0]));

    /* apply centered curve boost/decay */
    for(j=0;j<P_LEVELS;j++){
      for(k=0;k<EHMER_MAX;k++){
        float adj=center_boost+abs(EHMER_OFFSET-k)*center_decay_rate;
        if(adj<0. && center_boost>0)adj=0.;
        if(adj>0. && center_boost<0)adj=0.;
        workc[i][j][k]+=adj;
      }
    }

    /* normalize curves so the driving amplitude is 0dB */
    /* make temp curves with the ATH overlayed */
    for(j=0;j<P_LEVELS;j++){
      attenuate_curve(workc[i][j],curveatt_dB[i]+100.-(j<2?2:j)*10.-P_LEVEL_0);
      memcpy(athc[j],ath,EHMER_MAX*sizeof(**athc));
      attenuate_curve(athc[j],+100.-j*10.f-P_LEVEL_0);
      max_curve(athc[j],workc[i][j]);
    }

    /* Now limit the louder curves.

       the idea is this: We don't know what the playback attenuation
       will be; 0dB SL moves every time the user twiddles the volume
       knob. So that means we have to use a single 'most pessimal' curve
       for all masking amplitudes, right?  Wrong.  The *loudest* sound
       can be in (we assume) a range of ...+100dB] SL.  However, sounds
       20dB down will be in a range ...+80], 40dB down is from ...+60],
       etc... */

    for(j=1;j<P_LEVELS;j++){
      min_curve(athc[j],athc[j-1]);
      min_curve(workc[i][j],athc[j]);
    }
  }

  for(i=0;i<P_BANDS;i++){
    int hi_curve,lo_curve,bin;
    ret[i]=(float**)_ogg_malloc(sizeof(**ret)*P_LEVELS);

    /* low frequency curves are measured with greater resolution than
       the MDCT/FFT will actually give us; we want the curve applied
       to the tone data to be pessimistic and thus apply the minimum
       masking possible for a given bin.  That means that a single bin
       could span more than one octave and that the curve will be a
       composite of multiple octaves.  It also may mean that a single
       bin may span > an eighth of an octave and that the eighth
       octave values may also be composited. */

    /* which octave curves will we be compositing? */
    bin=floor(fromOC(i*.5)/binHz);
    lo_curve=  ceil(toOC(bin*binHz+1)*2);
    hi_curve=  floor(toOC((bin+1)*binHz)*2);
    if(lo_curve>i)lo_curve=i;
    if(lo_curve<0)lo_curve=0;
    if(hi_curve>=P_BANDS)hi_curve=P_BANDS-1;

    for(m=0;m<P_LEVELS;m++){
      ret[i][m]=(float*)_ogg_malloc(sizeof(***ret)*(EHMER_MAX+2));

      for(j=0;j<n;j++)brute_buffer[j]=999.;

      /* render the curve into bins, then pull values back into curve.
         The point is that any inherent subsampling aliasing results in
         a safe minimum */
      for(k=lo_curve;k<=hi_curve;k++){
        int l=0;

        for(j=0;j<EHMER_MAX;j++){
          int lo_bin= fromOC(j*.125+k*.5-2.0625)/binHz;
          int hi_bin= fromOC(j*.125+k*.5-1.9375)/binHz+1;

          if(lo_bin<0)lo_bin=0;
          if(lo_bin>n)lo_bin=n;
          if(lo_bin<l)l=lo_bin;
          if(hi_bin<0)hi_bin=0;
          if(hi_bin>n)hi_bin=n;

          for(;l<hi_bin && l<n;l++)
            if(brute_buffer[l]>workc[k][m][j])
              brute_buffer[l]=workc[k][m][j];
        }

        for(;l<n;l++)
          if(brute_buffer[l]>workc[k][m][EHMER_MAX-1])
            brute_buffer[l]=workc[k][m][EHMER_MAX-1];

      }

      /* be equally paranoid about being valid up to next half ocatve */
      if(i+1<P_BANDS){
        int l=0;
        k=i+1;
        for(j=0;j<EHMER_MAX;j++){
          int lo_bin= fromOC(j*.125+i*.5-2.0625)/binHz;
          int hi_bin= fromOC(j*.125+i*.5-1.9375)/binHz+1;

          if(lo_bin<0)lo_bin=0;
          if(lo_bin>n)lo_bin=n;
          if(lo_bin<l)l=lo_bin;
          if(hi_bin<0)hi_bin=0;
          if(hi_bin>n)hi_bin=n;

          for(;l<hi_bin && l<n;l++)
            if(brute_buffer[l]>workc[k][m][j])
              brute_buffer[l]=workc[k][m][j];
        }

        for(;l<n;l++)
          if(brute_buffer[l]>workc[k][m][EHMER_MAX-1])
            brute_buffer[l]=workc[k][m][EHMER_MAX-1];

      }


      for(j=0;j<EHMER_MAX;j++){
        int bin=fromOC(j*.125+i*.5-2.)/binHz;
        if(bin<0){
          ret[i][m][j+2]=-999.;
        }else{
          if(bin>=n){
            ret[i][m][j+2]=-999.;
          }else{
            ret[i][m][j+2]=brute_buffer[bin];
          }
        }
      }

      /* add fenceposts */
      for(j=0;j<EHMER_OFFSET;j++)
        if(ret[i][m][j+2]>-200.f)break;
      ret[i][m][0]=j;

      for(j=EHMER_MAX-1;j>EHMER_OFFSET+1;j--)
        if(ret[i][m][j+2]>-200.f)
          break;
      ret[i][m][1]=j;

    }
  }

  return(ret);
}

void _vp_psy_init(vorbis_look_psy *p,vorbis_info_psy *vi,
                  vorbis_info_psy_global *gi,int n,long rate){
  long i,j,lo=-99,hi=1;
  long maxoc;
  memset(p,0,sizeof(*p));

  p->eighth_octave_lines=gi->eighth_octave_lines;
  p->shiftoc=rint(log(gi->eighth_octave_lines*8.f)/log(2.f))-1;

  p->firstoc=toOC(.25f*rate*.5/n)*(1<<(p->shiftoc+1))-gi->eighth_octave_lines;
  maxoc=toOC((n+.25f)*rate*.5/n)*(1<<(p->shiftoc+1))+.5f;
  p->total_octave_lines=maxoc-p->firstoc+1;
  p->ath=(float*)_ogg_malloc(n*sizeof(*p->ath));

  p->octave=(long*)_ogg_malloc(n*sizeof(*p->octave));
  p->bark=(long*)_ogg_malloc(n*sizeof(*p->bark));
  p->vi=vi;
  p->n=n;
  p->rate=rate;

  /* AoTuV HF weighting */
  p->m_val = 1.;
  if(rate < 26000) p->m_val = 0;
  else if(rate < 38000) p->m_val = .94;   /* 32kHz */
  else if(rate > 46000) p->m_val = 1.275; /* 48kHz */

  /* set up the lookups for a given blocksize and sample rate */

  for(i=0,j=0;i<MAX_ATH-1;i++){
    int endpos=rint(fromOC((i+1)*.125-2.)*2*n/rate);
    float base=ATH[i];
    if(j<endpos){
      float delta=(ATH[i+1]-base)/(endpos-j);
      for(;j<endpos && j<n;j++){
        p->ath[j]=base+100.;
        base+=delta;
      }
    }
  }

  for(;j<n;j++){
    p->ath[j]=p->ath[j-1];
  }

  for(i=0;i<n;i++){
    float bark=toBARK(rate/(2*n)*i);

    for(;lo+vi->noisewindowlomin<i &&
          toBARK(rate/(2*n)*lo)<(bark-vi->noisewindowlo);lo++);

    for(;hi<=n && (hi<i+vi->noisewindowhimin ||
          toBARK(rate/(2*n)*hi)<(bark+vi->noisewindowhi));hi++);

    p->bark[i]=((lo-1)<<16)+(hi-1);

  }

  for(i=0;i<n;i++)
    p->octave[i]=toOC((i+.25f)*.5*rate/n)*(1<<(p->shiftoc+1))+.5f;

  p->tonecurves=setup_tone_curves(vi->toneatt,rate*.5/n,n,
                                  vi->tone_centerboost,vi->tone_decay);

  /* set up rolling noise median */
  p->noiseoffset=(float**)_ogg_malloc(P_NOISECURVES*sizeof(*p->noiseoffset));
  for(i=0;i<P_NOISECURVES;i++)
    p->noiseoffset[i]=(float*)_ogg_malloc(n*sizeof(**p->noiseoffset));

  for(i=0;i<n;i++){
    float halfoc=toOC((i+.5)*rate/(2.*n))*2.;
    int inthalfoc;
    float del;

    if(halfoc<0)halfoc=0;
    if(halfoc>=P_BANDS-1)halfoc=P_BANDS-1;
    inthalfoc=(int)halfoc;
    del=halfoc-inthalfoc;

    for(j=0;j<P_NOISECURVES;j++)
      p->noiseoffset[j][i]=
        p->vi->noiseoff[j][inthalfoc]*(1.-del) +
        p->vi->noiseoff[j][inthalfoc+1]*del;

  }
#if 0
  {
    static int ls=0;
    _analysis_output_always("noiseoff0",ls,p->noiseoffset[0],n,1,0,0);
    _analysis_output_always("noiseoff1",ls,p->noiseoffset[1],n,1,0,0);
    _analysis_output_always("noiseoff2",ls++,p->noiseoffset[2],n,1,0,0);
  }
#endif
}

void _vp_psy_clear(vorbis_look_psy *p){
  int i,j;
  if(p){
    if(p->ath)_ogg_free(p->ath);
    if(p->octave)_ogg_free(p->octave);
    if(p->bark)_ogg_free(p->bark);
    if(p->tonecurves){
      for(i=0;i<P_BANDS;i++){
        for(j=0;j<P_LEVELS;j++){
          _ogg_free(p->tonecurves[i][j]);
        }
        _ogg_free(p->tonecurves[i]);
      }
      _ogg_free(p->tonecurves);
    }
    if(p->noiseoffset){
      for(i=0;i<P_NOISECURVES;i++){
        _ogg_free(p->noiseoffset[i]);
      }
      _ogg_free(p->noiseoffset);
    }
    memset(p,0,sizeof(*p));
  }
}

/* octave/(8*eighth_octave_lines) x scale and dB y scale */
static void seed_curve(float *seed,
                       const float **curves,
                       float amp,
                       int oc, int n,
                       int linesper,float dBoffset){
  int i,post1;
  int seedptr;
  const float *posts,*curve;

  int choice=(int)((amp+dBoffset-P_LEVEL_0)*.1f);
  choice=max(choice,0);
  choice=min(choice,P_LEVELS-1);
  posts=curves[choice];
  curve=posts+2;
  post1=(int)posts[1];
  seedptr=oc+(posts[0]-EHMER_OFFSET)*linesper-(linesper>>1);

  for(i=posts[0];i<post1;i++){
    if(seedptr>0){
      float lin=amp+curve[i];
      if(seed[seedptr]<lin)seed[seedptr]=lin;
    }
    seedptr+=linesper;
    if(seedptr>=n)break;
  }
}

static void seed_loop(vorbis_look_psy *p,
                      const float ***curves,
                      const float *f,
                      const float *flr,
                      float *seed,
                      float specmax){
  vorbis_info_psy *vi=p->vi;
  long n=p->n,i;
  float dBoffset=vi->max_curve_dB-specmax;

  /* prime the working vector with peak values */

  for(i=0;i<n;i++){
    float max=f[i];
    long oc=p->octave[i];
    while(i+1<n && p->octave[i+1]==oc){
      i++;
      if(f[i]>max)max=f[i];
    }

    if(max+6.f>flr[i]){
      oc=oc>>p->shiftoc;

      if(oc>=P_BANDS)oc=P_BANDS-1;
      if(oc<0)oc=0;

      seed_curve(seed,
                 curves[oc],
                 max,
                 p->octave[i]-p->firstoc,
                 p->total_octave_lines,
                 p->eighth_octave_lines,
                 dBoffset);
    }
  }
}

static void seed_chase(float *seeds, int linesper, long n){
  long  *posstack=(long*)alloca(n*sizeof(*posstack));
  float *ampstack=(float*)alloca(n*sizeof(*ampstack));
  long   stack=0;
  long   pos=0;
  long   i;

  for(i=0;i<n;i++){
    if(stack<2){
      posstack[stack]=i;
      ampstack[stack++]=seeds[i];
    }else{
      while(1){
        if(seeds[i]<ampstack[stack-1]){
          posstack[stack]=i;
          ampstack[stack++]=seeds[i];
          break;
        }else{
          if(i<posstack[stack-1]+linesper){
            if(stack>1 && ampstack[stack-1]<=ampstack[stack-2] &&
               i<posstack[stack-2]+linesper){
              /* we completely overlap, making stack-1 irrelevant.  pop it */
              stack--;
              continue;
            }
          }
          posstack[stack]=i;
          ampstack[stack++]=seeds[i];
          break;

        }
      }
    }
  }

  /* the stack now contains only the positions that are relevant. Scan
     'em straight through */

  for(i=0;i<stack;i++){
    long endpos;
    if(i<stack-1 && ampstack[i+1]>ampstack[i]){
      endpos=posstack[i+1];
    }else{
      endpos=posstack[i]+linesper+1; /* +1 is important, else bin 0 is
                                        discarded in short frames */
    }
    if(endpos>n)endpos=n;
    for(;pos<endpos;pos++)
      seeds[pos]=ampstack[i];
  }

  /* there.  Linear time.  I now remember this was on a problem set I
     had in Grad Skool... I didn't solve it at the time ;-) */

}

/* bleaugh, this is more complicated than it needs to be */
#include<stdio.h>
static void max_seeds(vorbis_look_psy *p,
                      float *seed,
                      float *flr){
  long   n=p->total_octave_lines;
  int    linesper=p->eighth_octave_lines;
  long   linpos=0;
  long   pos;

  seed_chase(seed,linesper,n); /* for masking */

  pos=p->octave[0]-p->firstoc-(linesper>>1);

  while(linpos+1<p->n){
    float minV=seed[pos];
    long end=((p->octave[linpos]+p->octave[linpos+1])>>1)-p->firstoc;
    if(minV>p->vi->tone_abs_limit)minV=p->vi->tone_abs_limit;
    while(pos+1<=end){
      pos++;
      if((seed[pos]>NEGINF && seed[pos]<minV) || minV==NEGINF)
        minV=seed[pos];
    }

    end=pos+p->firstoc;
    for(;linpos<p->n && p->octave[linpos]<=end;linpos++)
      if(flr[linpos]<minV)flr[linpos]=minV;
  }

  {
    float minV=seed[p->total_octave_lines-1];
    for(;linpos<p->n;linpos++)
      if(flr[linpos]<minV)flr[linpos]=minV;
  }

}

static void bark_noise_hybridmp(int n,const long *b,
                                const float *f,
                                float *noise,
                                const float offset,
                                const int fixed){

  float *N=(float*)alloca(n*sizeof(*N));
  float *X=(float*)alloca(n*sizeof(*N));
  float *XX=(float*)alloca(n*sizeof(*N));
  float *Y=(float*)alloca(n*sizeof(*N));
  float *XY=(float*)alloca(n*sizeof(*N));

  float tN, tX, tXX, tY, tXY;
  int i;

  int lo, hi;
  float R=0.f;
  float A=0.f;
  float B=0.f;
  float D=1.f;
  float w, x, y;

  tN = tX = tXX = tY = tXY = 0.f;

  y = f[0] + offset;
  if (y < 1.f) y = 1.f;

  w = y * y * .5;

  tN += w;
  tX += w;
  tY += w * y;

  N[0] = tN;
  X[0] = tX;
  XX[0] = tXX;
  Y[0] = tY;
  XY[0] = tXY;

  for (i = 1, x = 1.f; i < n; i++, x += 1.f) {

    y = f[i] + offset;
    if (y < 1.f) y = 1.f;

    w = y * y;

    tN += w;
    tX += w * x;
    tXX += w * x * x;
    tY += w * y;
    tXY += w * x * y;

    N[i] = tN;
    X[i] = tX;
    XX[i] = tXX;
    Y[i] = tY;
    XY[i] = tXY;
  }

  for (i = 0, x = 0.f; i < n; i++, x += 1.f) {

    lo = b[i] >> 16;
    hi = b[i] & 0xffff;
    if( lo>=0 || -lo>=n ) break;
    if( hi>=n ) break;

    tN = N[hi] + N[-lo];
    tX = X[hi] - X[-lo];
    tXX = XX[hi] + XX[-lo];
    tY = Y[hi] + Y[-lo];
    tXY = XY[hi] - XY[-lo];

    A = tY * tXX - tX * tXY;
    B = tN * tXY - tX * tY;
    D = tN * tXX - tX * tX;
    R = (A + x * B) / D;
    if (R < 0.f) R = 0.f;

    noise[i] = R - offset;
  }

  for ( ; i < n; i++, x += 1.f) {

    lo = b[i] >> 16;
    hi = b[i] & 0xffff;
    if( lo<0 || lo>=n ) break;
    if( hi>=n ) break;

    tN = N[hi] - N[lo];
    tX = X[hi] - X[lo];
    tXX = XX[hi] - XX[lo];
    tY = Y[hi] - Y[lo];
    tXY = XY[hi] - XY[lo];

    A = tY * tXX - tX * tXY;
    B = tN * tXY - tX * tY;
    D = tN * tXX - tX * tX;
    R = (A + x * B) / D;
    if (R < 0.f) R = 0.f;

    noise[i] = R - offset;
  }

  for ( ; i < n; i++, x += 1.f) {

    R = (A + x * B) / D;
    if (R < 0.f) R = 0.f;

    noise[i] = R - offset;
  }

  if (fixed <= 0) return;

  for (i = 0, x = 0.f; i < n; i++, x += 1.f) {
    hi = i + fixed / 2;
    lo = hi - fixed;
    if ( hi>=n ) break;
    if ( lo>=0 ) break;

    tN = N[hi] + N[-lo];
    tX = X[hi] - X[-lo];
    tXX = XX[hi] + XX[-lo];
    tY = Y[hi] + Y[-lo];
    tXY = XY[hi] - XY[-lo];


    A = tY * tXX - tX * tXY;
    B = tN * tXY - tX * tY;
    D = tN * tXX - tX * tX;
    R = (A + x * B) / D;

    if (R - offset < noise[i]) noise[i] = R - offset;
  }
  for ( ; i < n; i++, x += 1.f) {

    hi = i + fixed / 2;
    lo = hi - fixed;
    if ( hi>=n ) break;
    if ( lo<0 ) break;

    tN = N[hi] - N[lo];
    tX = X[hi] - X[lo];
    tXX = XX[hi] - XX[lo];
    tY = Y[hi] - Y[lo];
    tXY = XY[hi] - XY[lo];

    A = tY * tXX - tX * tXY;
    B = tN * tXY - tX * tY;
    D = tN * tXX - tX * tX;
    R = (A + x * B) / D;

    if (R - offset < noise[i]) noise[i] = R - offset;
  }
  for ( ; i < n; i++, x += 1.f) {
    R = (A + x * B) / D;
    if (R - offset < noise[i]) noise[i] = R - offset;
  }
}

void _vp_noisemask(vorbis_look_psy *p,
                   float *logmdct,
                   float *logmask){

  int i,n=p->n;
  float *work=(float*)alloca(n*sizeof(*work));

  bark_noise_hybridmp(n,p->bark,logmdct,logmask,
                      140.,-1);

  for(i=0;i<n;i++)work[i]=logmdct[i]-logmask[i];

  bark_noise_hybridmp(n,p->bark,work,logmask,0.,
                      p->vi->noisewindowfixed);

  for(i=0;i<n;i++)work[i]=logmdct[i]-work[i];

#if 0
  {
    static int seq=0;

    float work2[n];
    for(i=0;i<n;i++){
      work2[i]=logmask[i]+work[i];
    }

    if(seq&1)
      _analysis_output("median2R",seq/2,work,n,1,0,0);
    else
      _analysis_output("median2L",seq/2,work,n,1,0,0);

    if(seq&1)
      _analysis_output("envelope2R",seq/2,work2,n,1,0,0);
    else
      _analysis_output("envelope2L",seq/2,work2,n,1,0,0);
    seq++;
  }
#endif

  for(i=0;i<n;i++){
    int dB=logmask[i]+.5;
    if(dB>=NOISE_COMPAND_LEVELS)dB=NOISE_COMPAND_LEVELS-1;
    if(dB<0)dB=0;
    logmask[i]= work[i]+p->vi->noisecompand[dB];
  }

}

void _vp_tonemask(vorbis_look_psy *p,
                  float *logfft,
                  float *logmask,
                  float global_specmax,
                  float local_specmax){

  int i,n=p->n;

  float *seed=(float*)alloca(sizeof(*seed)*p->total_octave_lines);
  float att=local_specmax+p->vi->ath_adjatt;
  for(i=0;i<p->total_octave_lines;i++)seed[i]=NEGINF;

  /* set the ATH (floating below localmax, not global max by a
     specified att) */
  if(att<p->vi->ath_maxatt)att=p->vi->ath_maxatt;

  for(i=0;i<n;i++)
    logmask[i]=p->ath[i]+att;

  /* tone masking */
  seed_loop(p,(const float ***)p->tonecurves,logfft,logmask,seed,global_specmax);
  max_seeds(p,seed,logmask);

}

void _vp_offset_and_mix(vorbis_look_psy *p,
                        float *noise,
                        float *tone,
                        int offset_select,
                        float *logmask,
                        float *mdct,
                        float *logmdct){
  int i,n=p->n;
  float de, coeffi, cx;/* AoTuV */
  float toneatt=p->vi->tone_masteratt[offset_select];

  cx = p->m_val;

  for(i=0;i<n;i++){
    float val= noise[i]+p->noiseoffset[offset_select][i];
    if(val>p->vi->noisemaxsupp)val=p->vi->noisemaxsupp;
    logmask[i]=max(val,tone[i]+toneatt);


    /* AoTuV */
    /** @ M1 **
        The following codes improve a noise problem.
        A fundamental idea uses the value of masking and carries out
        the relative compensation of the MDCT.
        However, this code is not perfect and all noise problems cannot be solved.
        by Aoyumi @ 2004/04/18
    */

    if(offset_select == 1) {
      coeffi = -17.2;       /* coeffi is a -17.2dB threshold */
      val = val - logmdct[i];  /* val == mdct line value relative to floor in dB */

      if(val > coeffi){
        /* mdct value is > -17.2 dB below floor */

        de = 1.0-((val-coeffi)*0.005*cx);
        /* pro-rated attenuation:
           -0.00 dB boost if mdct value is -17.2dB (relative to floor)
           -0.77 dB boost if mdct value is 0dB (relative to floor)
           -1.64 dB boost if mdct value is +17.2dB (relative to floor)
           etc... */

        if(de < 0) de = 0.0001;
      }else
        /* mdct value is <= -17.2 dB below floor */

        de = 1.0-((val-coeffi)*0.0003*cx);
      /* pro-rated attenuation:
         +0.00 dB atten if mdct value is -17.2dB (relative to floor)
         +0.45 dB atten if mdct value is -34.4dB (relative to floor)
         etc... */

      mdct[i] *= de;

    }
  }
}

float _vp_ampmax_decay(float amp,vorbis_dsp_state *vd){
  vorbis_info *vi=vd->vi;
  codec_setup_info *ci=(codec_setup_info*)vi->codec_setup;
  vorbis_info_psy_global *gi=&ci->psy_g_param;

  int n=ci->blocksizes[vd->W]/2;
  float secs=(float)n/vi->rate;

  amp+=secs*gi->ampmax_att_per_sec;
  if(amp<-9999)amp=-9999;
  return(amp);
}

static float _FLOOR1_fromdB_LOOKUP[256]={
  1.0649863e-07F, 1.1341951e-07F, 1.2079015e-07F, 1.2863978e-07F,
  1.3699951e-07F, 1.4590251e-07F, 1.5538408e-07F, 1.6548181e-07F,
  1.7623575e-07F, 1.8768855e-07F, 1.9988561e-07F, 2.128753e-07F,
  2.2670913e-07F, 2.4144197e-07F, 2.5713223e-07F, 2.7384213e-07F,
  2.9163793e-07F, 3.1059021e-07F, 3.3077411e-07F, 3.5226968e-07F,
  3.7516214e-07F, 3.9954229e-07F, 4.2550680e-07F, 4.5315863e-07F,
  4.8260743e-07F, 5.1396998e-07F, 5.4737065e-07F, 5.8294187e-07F,
  6.2082472e-07F, 6.6116941e-07F, 7.0413592e-07F, 7.4989464e-07F,
  7.9862701e-07F, 8.5052630e-07F, 9.0579828e-07F, 9.6466216e-07F,
  1.0273513e-06F, 1.0941144e-06F, 1.1652161e-06F, 1.2409384e-06F,
  1.3215816e-06F, 1.4074654e-06F, 1.4989305e-06F, 1.5963394e-06F,
  1.7000785e-06F, 1.8105592e-06F, 1.9282195e-06F, 2.0535261e-06F,
  2.1869758e-06F, 2.3290978e-06F, 2.4804557e-06F, 2.6416497e-06F,
  2.8133190e-06F, 2.9961443e-06F, 3.1908506e-06F, 3.3982101e-06F,
  3.6190449e-06F, 3.8542308e-06F, 4.1047004e-06F, 4.3714470e-06F,
  4.6555282e-06F, 4.9580707e-06F, 5.2802740e-06F, 5.6234160e-06F,
  5.9888572e-06F, 6.3780469e-06F, 6.7925283e-06F, 7.2339451e-06F,
  7.7040476e-06F, 8.2047000e-06F, 8.7378876e-06F, 9.3057248e-06F,
  9.9104632e-06F, 1.0554501e-05F, 1.1240392e-05F, 1.1970856e-05F,
  1.2748789e-05F, 1.3577278e-05F, 1.4459606e-05F, 1.5399272e-05F,
  1.6400004e-05F, 1.7465768e-05F, 1.8600792e-05F, 1.9809576e-05F,
  2.1096914e-05F, 2.2467911e-05F, 2.3928002e-05F, 2.5482978e-05F,
  2.7139006e-05F, 2.8902651e-05F, 3.0780908e-05F, 3.2781225e-05F,
  3.4911534e-05F, 3.7180282e-05F, 3.9596466e-05F, 4.2169667e-05F,
  4.4910090e-05F, 4.7828601e-05F, 5.0936773e-05F, 5.4246931e-05F,
  5.7772202e-05F, 6.1526565e-05F, 6.5524908e-05F, 6.9783085e-05F,
  7.4317983e-05F, 7.9147585e-05F, 8.4291040e-05F, 8.9768747e-05F,
  9.5602426e-05F, 0.00010181521F, 0.00010843174F, 0.00011547824F,
  0.00012298267F, 0.00013097477F, 0.00013948625F, 0.00014855085F,
  0.00015820453F, 0.00016848555F, 0.00017943469F, 0.00019109536F,
  0.00020351382F, 0.00021673929F, 0.00023082423F, 0.00024582449F,
  0.00026179955F, 0.00027881276F, 0.00029693158F, 0.00031622787F,
  0.00033677814F, 0.00035866388F, 0.00038197188F, 0.00040679456F,
  0.00043323036F, 0.00046138411F, 0.00049136745F, 0.00052329927F,
  0.00055730621F, 0.00059352311F, 0.00063209358F, 0.00067317058F,
  0.00071691700F, 0.00076350630F, 0.00081312324F, 0.00086596457F,
  0.00092223983F, 0.00098217216F, 0.0010459992F, 0.0011139742F,
  0.0011863665F, 0.0012634633F, 0.0013455702F, 0.0014330129F,
  0.0015261382F, 0.0016253153F, 0.0017309374F, 0.0018434235F,
  0.0019632195F, 0.0020908006F, 0.0022266726F, 0.0023713743F,
  0.0025254795F, 0.0026895994F, 0.0028643847F, 0.0030505286F,
  0.0032487691F, 0.0034598925F, 0.0036847358F, 0.0039241906F,
  0.0041792066F, 0.0044507950F, 0.0047400328F, 0.0050480668F,
  0.0053761186F, 0.0057254891F, 0.0060975636F, 0.0064938176F,
  0.0069158225F, 0.0073652516F, 0.0078438871F, 0.0083536271F,
  0.0088964928F, 0.009474637F, 0.010090352F, 0.010746080F,
  0.011444421F, 0.012188144F, 0.012980198F, 0.013823725F,
  0.014722068F, 0.015678791F, 0.016697687F, 0.017782797F,
  0.018938423F, 0.020169149F, 0.021479854F, 0.022875735F,
  0.024362330F, 0.025945531F, 0.027631618F, 0.029427276F,
  0.031339626F, 0.033376252F, 0.035545228F, 0.037855157F,
  0.040315199F, 0.042935108F, 0.045725273F, 0.048696758F,
  0.051861348F, 0.055231591F, 0.058820850F, 0.062643361F,
  0.066714279F, 0.071049749F, 0.075666962F, 0.080584227F,
  0.085821044F, 0.091398179F, 0.097337747F, 0.10366330F,
  0.11039993F, 0.11757434F, 0.12521498F, 0.13335215F,
  0.14201813F, 0.15124727F, 0.16107617F, 0.17154380F,
  0.18269168F, 0.19456402F, 0.20720788F, 0.22067342F,
  0.23501402F, 0.25028656F, 0.26655159F, 0.28387361F,
  0.30232132F, 0.32196786F, 0.34289114F, 0.36517414F,
  0.38890521F, 0.41417847F, 0.44109412F, 0.46975890F,
  0.50028648F, 0.53279791F, 0.56742212F, 0.60429640F,
  0.64356699F, 0.68538959F, 0.72993007F, 0.77736504F,
  0.82788260F, 0.88168307F, 0.9389798F, 1.F,
};

/* this is for per-channel noise normalization */
static int apsort(const void *a, const void *b){
  float f1=**(float**)a;
  float f2=**(float**)b;
  return (f1<f2)-(f1>f2);
}

static void flag_lossless(int limit, float prepoint, float postpoint, float *mdct,
                         float *floor, int *flag, int i, int jn){
  int j;
  for(j=0;j<jn;j++){
    float point = j>=limit-i ? postpoint : prepoint;
    float r = fabs(mdct[j])/floor[j];
    if(r<point)
      flag[j]=0;
    else
      flag[j]=1;
  }
}

/* Overload/Side effect: On input, the *q vector holds either the
   quantized energy (for elements with the flag set) or the absolute
   values of the *r vector (for elements with flag unset).  On output,
   *q holds the quantized energy for all elements */
static float noise_normalize(vorbis_look_psy *p, int limit, float *r, float *q, float *f, int *flags, float acc, int i, int n, int *out){

  vorbis_info_psy *vi=p->vi;
  float **sort = (float**)alloca(n*sizeof(*sort));
  int j,count=0;
  int start = (vi->normal_p ? vi->normal_start-i : n);
  if(start>n)start=n;

  /* force classic behavior where only energy in the current band is considered */
  acc=0.f;

  /* still responsible for populating *out where noise norm not in
     effect.  There's no need to [re]populate *q in these areas */
  for(j=0;j<start;j++){
    if(!flags || !flags[j]){ /* lossless coupling already quantized.
                                Don't touch; requantizing based on
                                energy would be incorrect. */
      float ve = q[j]/f[j];
      if(r[j]<0)
        out[j] = -rint(sqrt(ve));
      else
        out[j] = rint(sqrt(ve));
    }
  }

  /* sort magnitudes for noise norm portion of partition */
  for(;j<n;j++){
    if(!flags || !flags[j]){ /* can't noise norm elements that have
                                already been loslessly coupled; we can
                                only account for their energy error */
      float ve = q[j]/f[j];
      /* Despite all the new, more capable coupling code, for now we
         implement noise norm as it has been up to this point. Only
         consider promotions to unit magnitude from 0.  In addition
         the only energy error counted is quantizations to zero. */
      /* also-- the original point code only applied noise norm at > pointlimit */
      if(ve<.25f && (!flags || j>=limit-i)){
        acc += ve;
        sort[count++]=q+j; /* q is fabs(r) for unflagged element */
      }else{
        /* For now: no acc adjustment for nonzero quantization.  populate *out and q as this value is final. */
        if(r[j]<0)
          out[j] = -rint(sqrt(ve));
        else
          out[j] = rint(sqrt(ve));
        q[j] = out[j]*out[j]*f[j];
      }
    }/* else{
        again, no energy adjustment for error in nonzero quant-- for now
        }*/
  }

  if(count){
    /* noise norm to do */
    qsort(sort,count,sizeof(*sort),apsort);
    for(j=0;j<count;j++){
      int k=sort[j]-q;
      if(acc>=vi->normal_thresh){
        out[k]=unitnorm(r[k]);
        acc-=1.f;
        q[k]=f[k];
      }else{
        out[k]=0;
        q[k]=0.f;
      }
    }
  }

  return acc;
}

/* Noise normalization, quantization and coupling are not wholly
   seperable processes in depth>1 coupling. */
void _vp_couple_quantize_normalize(int blobno,
                                   vorbis_info_psy_global *g,
                                   vorbis_look_psy *p,
                                   vorbis_info_mapping0 *vi,
                                   float **mdct,
                                   int   **iwork,
                                   int    *nonzero,
                                   int     sliding_lowpass,
                                   int     ch){

  int i;
  int n = p->n;
  int partition=(p->vi->normal_p ? p->vi->normal_partition : 16);
  int limit = g->coupling_pointlimit[p->vi->blockflag][blobno];
  float prepoint=stereo_threshholds[g->coupling_prepointamp[blobno]];
  float postpoint=stereo_threshholds[g->coupling_postpointamp[blobno]];
#if 0
  float de=0.1*p->m_val; /* a blend of the AoTuV M2 and M3 code here and below */
#endif

  /* mdct is our raw mdct output, floor not removed. */
  /* inout passes in the ifloor, passes back quantized result */

  /* unquantized energy (negative indicates amplitude has negative sign) */
  float **raw = (float**)alloca(ch*sizeof(*raw));

  /* dual pupose; quantized energy (if flag set), othersize fabs(raw) */
  float **quant = (float**)alloca(ch*sizeof(*quant));

  /* floor energy */
  float **floor = (float**)alloca(ch*sizeof(*floor));

  /* flags indicating raw/quantized status of elements in raw vector */
  int   **flag  = (int**)alloca(ch*sizeof(*flag));

  /* non-zero flag working vector */
  int    *nz    = (int*)alloca(ch*sizeof(*nz));

  /* energy surplus/defecit tracking */
  float  *acc   = (float*)alloca((ch+vi->coupling_steps)*sizeof(*acc));

  /* The threshold of a stereo is changed with the size of n */
  if(n > 1000)
    postpoint=stereo_threshholds_limited[g->coupling_postpointamp[blobno]];

  raw[0]   = (float*)alloca(ch*partition*sizeof(**raw));
  quant[0] = (float*)alloca(ch*partition*sizeof(**quant));
  floor[0] = (float*)alloca(ch*partition*sizeof(**floor));
  flag[0]  = (int*)alloca(ch*partition*sizeof(**flag));

  for(i=1;i<ch;i++){
    raw[i]   = &raw[0][partition*i];
    quant[i] = &quant[0][partition*i];
    floor[i] = &floor[0][partition*i];
    flag[i]  = &flag[0][partition*i];
  }
  for(i=0;i<ch+vi->coupling_steps;i++)
    acc[i]=0.f;

  for(i=0;i<n;i+=partition){
    int k,j,jn = partition > n-i ? n-i : partition;
    int step,track = 0;

    memcpy(nz,nonzero,sizeof(*nz)*ch);

    /* prefill */
    memset(flag[0],0,ch*partition*sizeof(**flag));
    for(k=0;k<ch;k++){
      int *iout = &iwork[k][i];
      if(nz[k]){

        for(j=0;j<jn;j++)
          floor[k][j] = _FLOOR1_fromdB_LOOKUP[iout[j]];

        flag_lossless(limit,prepoint,postpoint,&mdct[k][i],floor[k],flag[k],i,jn);

        for(j=0;j<jn;j++){
          quant[k][j] = raw[k][j] = mdct[k][i+j]*mdct[k][i+j];
          if(mdct[k][i+j]<0.f) raw[k][j]*=-1.f;
          floor[k][j]*=floor[k][j];
        }

        acc[track]=noise_normalize(p,limit,raw[k],quant[k],floor[k],NULL,acc[track],i,jn,iout);

      }else{
        for(j=0;j<jn;j++){
          floor[k][j] = 1e-10f;
          raw[k][j] = 0.f;
          quant[k][j] = 0.f;
          flag[k][j] = 0;
          iout[j]=0;
        }
        acc[track]=0.f;
      }
      track++;
    }

    /* coupling */
    for(step=0;step<vi->coupling_steps;step++){
      int Mi = vi->coupling_mag[step];
      int Ai = vi->coupling_ang[step];
      int *iM = &iwork[Mi][i];
      int *iA = &iwork[Ai][i];
      float *reM = raw[Mi];
      float *reA = raw[Ai];
      float *qeM = quant[Mi];
      float *qeA = quant[Ai];
      float *floorM = floor[Mi];
      float *floorA = floor[Ai];
      int *fM = flag[Mi];
      int *fA = flag[Ai];

      if(nz[Mi] || nz[Ai]){
        nz[Mi] = nz[Ai] = 1;

        for(j=0;j<jn;j++){

          if(j<sliding_lowpass-i){
            if(fM[j] || fA[j]){
              /* lossless coupling */

              reM[j] = fabs(reM[j])+fabs(reA[j]);
              qeM[j] = qeM[j]+qeA[j];
              fM[j]=fA[j]=1;

              /* couple iM/iA */
              {
                int A = iM[j];
                int B = iA[j];

                if(abs(A)>abs(B)){
                  iA[j]=(A>0?A-B:B-A);
                }else{
                  iA[j]=(B>0?A-B:B-A);
                  iM[j]=B;
                }

                /* collapse two equivalent tuples to one */
                if(iA[j]>=abs(iM[j])*2){
                  iA[j]= -iA[j];
                  iM[j]= -iM[j];
                }

              }

            }else{
              /* lossy (point) coupling */
              if(j<limit-i){
                /* dipole */
                reM[j] += reA[j];
                qeM[j] = fabs(reM[j]);
              }else{
#if 0
                /* AoTuV */
                /** @ M2 **
                    The boost problem by the combination of noise normalization and point stereo is eased.
                    However, this is a temporary patch.
                    by Aoyumi @ 2004/04/18
                */
                float derate = (1.0 - de*((float)(j-limit+i) / (float)(n-limit)));
                /* elliptical */
                if(reM[j]+reA[j]<0){
                  reM[j] = - (qeM[j] = (fabs(reM[j])+fabs(reA[j]))*derate*derate);
                }else{
                  reM[j] =   (qeM[j] = (fabs(reM[j])+fabs(reA[j]))*derate*derate);
                }
#else
                /* elliptical */
                if(reM[j]+reA[j]<0){
                  reM[j] = - (qeM[j] = fabs(reM[j])+fabs(reA[j]));
                }else{
                  reM[j] =   (qeM[j] = fabs(reM[j])+fabs(reA[j]));
                }
#endif

              }
              reA[j]=qeA[j]=0.f;
              fA[j]=1;
              iA[j]=0;
            }
          }
          floorM[j]=floorA[j]=floorM[j]+floorA[j];
        }
        /* normalize the resulting mag vector */
        acc[track]=noise_normalize(p,limit,raw[Mi],quant[Mi],floor[Mi],flag[Mi],acc[track],i,jn,iM);
        track++;
      }
    }
  }

  for(i=0;i<vi->coupling_steps;i++){
    /* make sure coupling a zero and a nonzero channel results in two
       nonzero channels. */
    if(nonzero[vi->coupling_mag[i]] ||
       nonzero[vi->coupling_ang[i]]){
      nonzero[vi->coupling_mag[i]]=1;
      nonzero[vi->coupling_ang[i]]=1;
    }
  }
}
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2015             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: maintain the info structure, info <-> header packets

 ********************************************************************/

/* general handling of the header and the vorbis_info structure (and
   substructures) */

#include <stdlib.h>
#include <string.h>
/*#include <ogg/ogg.h>*/
/*#include "vorbis/codec.h"*/
/*#include "codec_internal.h"*/
/*#include "codebook.h"*/
/*#include "registry.h"*/
/*#include "window.h"*/
/*#include "psy.h"*/
/*#include "misc.h"*/
/*#include "os.h"*/

#define GENERAL_VENDOR_STRING "Xiph.Org libVorbis 1.3.7"
#define ENCODE_VENDOR_STRING "Xiph.Org libVorbis I 20200704 (Reducing Environment)"

/* helpers */
static void _v_writestring(oggpack_buffer *o,const char *s, int bytes){

  while(bytes--){
    oggpack_write(o,*s++,8);
  }
}

static void _v_readstring(oggpack_buffer *o,char *buf,int bytes){
  while(bytes--){
    *buf++=oggpack_read(o,8);
  }
}

static int _v_toupper(int c) {
  return (c >= 'a' && c <= 'z') ? (c & ~('a' - 'A')) : c;
}

void vorbis_comment_init(vorbis_comment *vc){
  memset(vc,0,sizeof(*vc));
}

void vorbis_comment_add(vorbis_comment *vc,const char *comment){
  vc->user_comments=(char**)_ogg_realloc(vc->user_comments,
                            (vc->comments+2)*sizeof(*vc->user_comments));
  vc->comment_lengths=(int*)_ogg_realloc(vc->comment_lengths,
                                  (vc->comments+2)*sizeof(*vc->comment_lengths));
  vc->comment_lengths[vc->comments]=strlen(comment);
  vc->user_comments[vc->comments]=(char*)_ogg_malloc(vc->comment_lengths[vc->comments]+1);
  strcpy(vc->user_comments[vc->comments], comment);
  vc->comments++;
  vc->user_comments[vc->comments]=NULL;
}

void vorbis_comment_add_tag(vorbis_comment *vc, const char *tag, const char *contents){
  /* Length for key and value +2 for = and \0 */
  char *comment=(char*)_ogg_malloc(strlen(tag)+strlen(contents)+2);
  strcpy(comment, tag);
  strcat(comment, "=");
  strcat(comment, contents);
  vorbis_comment_add(vc, comment);
  _ogg_free(comment);
}

/* This is more or less the same as strncasecmp - but that doesn't exist
 * everywhere, and this is a fairly trivial function, so we include it */
static int tagcompare(const char *s1, const char *s2, int n){
  int c=0;
  while(c < n){
    if(_v_toupper(s1[c]) != _v_toupper(s2[c]))
      return !0;
    c++;
  }
  return 0;
}

char *vorbis_comment_query(vorbis_comment *vc, const char *tag, int count){
  long i;
  int found = 0;
  int taglen = strlen(tag)+1; /* +1 for the = we append */
  char *fulltag = (char*)_ogg_malloc(taglen+1);

  strcpy(fulltag, tag);
  strcat(fulltag, "=");

  for(i=0;i<vc->comments;i++){
    if(!tagcompare(vc->user_comments[i], fulltag, taglen)){
      if(count == found) {
        /* We return a pointer to the data, not a copy */
        _ogg_free(fulltag);
        return vc->user_comments[i] + taglen;
      } else {
        found++;
      }
    }
  }
  _ogg_free(fulltag);
  return NULL; /* didn't find anything */
}

int vorbis_comment_query_count(vorbis_comment *vc, const char *tag){
  int i,count=0;
  int taglen = strlen(tag)+1; /* +1 for the = we append */
  char *fulltag = (char*)_ogg_malloc(taglen+1);
  strcpy(fulltag,tag);
  strcat(fulltag, "=");

  for(i=0;i<vc->comments;i++){
    if(!tagcompare(vc->user_comments[i], fulltag, taglen))
      count++;
  }

  _ogg_free(fulltag);
  return count;
}

void vorbis_comment_clear(vorbis_comment *vc){
  if(vc){
    long i;
    if(vc->user_comments){
      for(i=0;i<vc->comments;i++)
        if(vc->user_comments[i])_ogg_free(vc->user_comments[i]);
      _ogg_free(vc->user_comments);
    }
    if(vc->comment_lengths)_ogg_free(vc->comment_lengths);
    if(vc->vendor)_ogg_free(vc->vendor);
    memset(vc,0,sizeof(*vc));
  }
}

/* blocksize 0 is guaranteed to be short, 1 is guaranteed to be long.
   They may be equal, but short will never ge greater than long */
int vorbis_info_blocksize(vorbis_info *vi,int zo){
  codec_setup_info *ci = (codec_setup_info*)vi->codec_setup;
  return ci ? ci->blocksizes[zo] : -1;
}

/* used by synthesis, which has a full, alloced vi */
void vorbis_info_init(vorbis_info *vi){
  memset(vi,0,sizeof(*vi));
  vi->codec_setup=_ogg_calloc(1,sizeof(codec_setup_info));
}

void vorbis_info_clear(vorbis_info *vi){
  codec_setup_info     *ci= (codec_setup_info*)vi->codec_setup;
  int i;

  if(ci){

    for(i=0;i<ci->modes;i++)
      if(ci->mode_param[i])_ogg_free(ci->mode_param[i]);

    for(i=0;i<ci->maps;i++) /* unpack does the range checking */
      if(ci->map_param[i]) /* this may be cleaning up an aborted
                              unpack, in which case the below type
                              cannot be trusted */
        _mapping_P[ci->map_type[i]]->free_info(ci->map_param[i]);

    for(i=0;i<ci->floors;i++) /* unpack does the range checking */
      if(ci->floor_param[i]) /* this may be cleaning up an aborted
                                unpack, in which case the below type
                                cannot be trusted */
        _floor_P[ci->floor_type[i]]->free_info(ci->floor_param[i]);

    for(i=0;i<ci->residues;i++) /* unpack does the range checking */
      if(ci->residue_param[i]) /* this may be cleaning up an aborted
                                  unpack, in which case the below type
                                  cannot be trusted */
        _residue_P[ci->residue_type[i]]->free_info(ci->residue_param[i]);

    for(i=0;i<ci->books;i++){
      if(ci->book_param[i]){
        /* knows if the book was not alloced */
        vorbis_staticbook_destroy(ci->book_param[i]);
      }
      if(ci->fullbooks)
        vorbis_book_clear(ci->fullbooks+i);
    }
    if(ci->fullbooks)
        _ogg_free(ci->fullbooks);

    for(i=0;i<ci->psys;i++)
      _vi_psy_free(ci->psy_param[i]);

    _ogg_free(ci);
  }

  memset(vi,0,sizeof(*vi));
}

/* Header packing/unpacking ********************************************/

static int _vorbis_unpack_info(vorbis_info *vi,oggpack_buffer *opb){
  codec_setup_info     *ci= (codec_setup_info*)vi->codec_setup;
  int bs;
  if(!ci)return(OV_EFAULT);

  vi->version=oggpack_read(opb,32);
  if(vi->version!=0)return(OV_EVERSION);

  vi->channels=oggpack_read(opb,8);
  vi->rate=oggpack_read(opb,32);

  vi->bitrate_upper=(ogg_int32_t)oggpack_read(opb,32);
  vi->bitrate_nominal=(ogg_int32_t)oggpack_read(opb,32);
  vi->bitrate_lower=(ogg_int32_t)oggpack_read(opb,32);

  bs = oggpack_read(opb,4);
  if(bs<0)goto err_out;
  ci->blocksizes[0]=1<<bs;
  bs = oggpack_read(opb,4);
  if(bs<0)goto err_out;
  ci->blocksizes[1]=1<<bs;

  if(vi->rate<1)goto err_out;
  if(vi->channels<1)goto err_out;
  if(ci->blocksizes[0]<64)goto err_out;
  if(ci->blocksizes[1]<ci->blocksizes[0])goto err_out;
  if(ci->blocksizes[1]>8192)goto err_out;

  if(oggpack_read(opb,1)!=1)goto err_out; /* EOP check */

  return(0);
 err_out:
  vorbis_info_clear(vi);
  return(OV_EBADHEADER);
}

static int _vorbis_unpack_comment(vorbis_comment *vc,oggpack_buffer *opb){
  int i;
  int vendorlen=oggpack_read(opb,32);
  if(vendorlen<0)goto err_out;
  if(vendorlen>opb->storage-8)goto err_out;
  vc->vendor=(char*)_ogg_calloc(vendorlen+1,1);
  _v_readstring(opb,vc->vendor,vendorlen);
  i=oggpack_read(opb,32);
  if(i<0)goto err_out;
  if(i>((opb->storage-oggpack_bytes(opb))>>2))goto err_out;
  vc->comments=i;
  vc->user_comments=(char**)_ogg_calloc(vc->comments+1,sizeof(*vc->user_comments));
  vc->comment_lengths=(int*)_ogg_calloc(vc->comments+1, sizeof(*vc->comment_lengths));

  for(i=0;i<vc->comments;i++){
    int len=oggpack_read(opb,32);
    if(len<0)goto err_out;
    if(len>opb->storage-oggpack_bytes(opb))goto err_out;
    vc->comment_lengths[i]=len;
    vc->user_comments[i]=(char*)_ogg_calloc(len+1,1);
    _v_readstring(opb,vc->user_comments[i],len);
  }
  if(oggpack_read(opb,1)!=1)goto err_out; /* EOP check */

  return(0);
 err_out:
  vorbis_comment_clear(vc);
  return(OV_EBADHEADER);
}

/* all of the real encoding details are here.  The modes, books,
   everything */
static int _vorbis_unpack_books(vorbis_info *vi,oggpack_buffer *opb){
  codec_setup_info     *ci= (codec_setup_info*)vi->codec_setup;
  int i;

  /* codebooks */
  ci->books=oggpack_read(opb,8)+1;
  if(ci->books<=0)goto err_out;
  for(i=0;i<ci->books;i++){
    ci->book_param[i]=vorbis_staticbook_unpack(opb);
    if(!ci->book_param[i])goto err_out;
  }

  /* time backend settings; hooks are unused */
  {
    int times=oggpack_read(opb,6)+1;
    if(times<=0)goto err_out;
    for(i=0;i<times;i++){
      int test=oggpack_read(opb,16);
      if(test<0 || test>=VI_TIMEB)goto err_out;
    }
  }

  /* floor backend settings */
  ci->floors=oggpack_read(opb,6)+1;
  if(ci->floors<=0)goto err_out;
  for(i=0;i<ci->floors;i++){
    ci->floor_type[i]=oggpack_read(opb,16);
    if(ci->floor_type[i]<0 || ci->floor_type[i]>=VI_FLOORB)goto err_out;
    ci->floor_param[i]=_floor_P[ci->floor_type[i]]->unpack(vi,opb);
    if(!ci->floor_param[i])goto err_out;
  }

  /* residue backend settings */
  ci->residues=oggpack_read(opb,6)+1;
  if(ci->residues<=0)goto err_out;
  for(i=0;i<ci->residues;i++){
    ci->residue_type[i]=oggpack_read(opb,16);
    if(ci->residue_type[i]<0 || ci->residue_type[i]>=VI_RESB)goto err_out;
    ci->residue_param[i]=_residue_P[ci->residue_type[i]]->unpack(vi,opb);
    if(!ci->residue_param[i])goto err_out;
  }

  /* map backend settings */
  ci->maps=oggpack_read(opb,6)+1;
  if(ci->maps<=0)goto err_out;
  for(i=0;i<ci->maps;i++){
    ci->map_type[i]=oggpack_read(opb,16);
    if(ci->map_type[i]<0 || ci->map_type[i]>=VI_MAPB)goto err_out;
    ci->map_param[i]=_mapping_P[ci->map_type[i]]->unpack(vi,opb);
    if(!ci->map_param[i])goto err_out;
  }

  /* mode settings */
  ci->modes=oggpack_read(opb,6)+1;
  if(ci->modes<=0)goto err_out;
  for(i=0;i<ci->modes;i++){
    ci->mode_param[i]=(vorbis_info_mode*)_ogg_calloc(1,sizeof(*ci->mode_param[i]));
    ci->mode_param[i]->blockflag=oggpack_read(opb,1);
    ci->mode_param[i]->windowtype=oggpack_read(opb,16);
    ci->mode_param[i]->transformtype=oggpack_read(opb,16);
    ci->mode_param[i]->mapping=oggpack_read(opb,8);

    if(ci->mode_param[i]->windowtype>=VI_WINDOWB)goto err_out;
    if(ci->mode_param[i]->transformtype>=VI_WINDOWB)goto err_out;
    if(ci->mode_param[i]->mapping>=ci->maps)goto err_out;
    if(ci->mode_param[i]->mapping<0)goto err_out;
  }

  if(oggpack_read(opb,1)!=1)goto err_out; /* top level EOP check */

  return(0);
 err_out:
  vorbis_info_clear(vi);
  return(OV_EBADHEADER);
}

/* Is this packet a vorbis ID header? */
int vorbis_synthesis_idheader(ogg_packet *op){
  oggpack_buffer opb;
  char buffer[6];

  if(op){
    oggpack_readinit(&opb,op->packet,op->bytes);

    if(!op->b_o_s)
      return(0); /* Not the initial packet */

    if(oggpack_read(&opb,8) != 1)
      return 0; /* not an ID header */

    memset(buffer,0,6);
    _v_readstring(&opb,buffer,6);
    if(memcmp(buffer,"vorbis",6))
      return 0; /* not vorbis */

    return 1;
  }

  return 0;
}

/* The Vorbis header is in three packets; the initial small packet in
   the first page that identifies basic parameters, a second packet
   with bitstream comments and a third packet that holds the
   codebook. */

int vorbis_synthesis_headerin(vorbis_info *vi,vorbis_comment *vc,ogg_packet *op){
  oggpack_buffer opb;

  if(op){
    oggpack_readinit(&opb,op->packet,op->bytes);

    /* Which of the three types of header is this? */
    /* Also verify header-ness, vorbis */
    {
      char buffer[6];
      int packtype=oggpack_read(&opb,8);
      memset(buffer,0,6);
      _v_readstring(&opb,buffer,6);
      if(memcmp(buffer,"vorbis",6)){
        /* not a vorbis header */
        return(OV_ENOTVORBIS);
      }
      switch(packtype){
      case 0x01: /* least significant *bit* is read first */
        if(!op->b_o_s){
          /* Not the initial packet */
          return(OV_EBADHEADER);
        }
        if(vi->rate!=0){
          /* previously initialized info header */
          return(OV_EBADHEADER);
        }

        return(_vorbis_unpack_info(vi,&opb));

      case 0x03: /* least significant *bit* is read first */
        if(vi->rate==0){
          /* um... we didn't get the initial header */
          return(OV_EBADHEADER);
        }
        if(vc->vendor!=NULL){
          /* previously initialized comment header */
          return(OV_EBADHEADER);
        }

        return(_vorbis_unpack_comment(vc,&opb));

      case 0x05: /* least significant *bit* is read first */
        if(vi->rate==0 || vc->vendor==NULL){
          /* um... we didn;t get the initial header or comments yet */
          return(OV_EBADHEADER);
        }
        if(vi->codec_setup==NULL){
          /* improperly initialized vorbis_info */
          return(OV_EFAULT);
        }
        if(((codec_setup_info *)vi->codec_setup)->books>0){
          /* previously initialized setup header */
          return(OV_EBADHEADER);
        }

        return(_vorbis_unpack_books(vi,&opb));

      default:
        /* Not a valid vorbis header type */
        return(OV_EBADHEADER);
        break;
      }
    }
  }
  return(OV_EBADHEADER);
}

/* pack side **********************************************************/

static int _vorbis_pack_info(oggpack_buffer *opb,vorbis_info *vi){
  codec_setup_info     *ci=(codec_setup_info*)vi->codec_setup;
  if(!ci||
     ci->blocksizes[0]<64||
     ci->blocksizes[1]<ci->blocksizes[0]){
    return(OV_EFAULT);
  }

  /* preamble */
  oggpack_write(opb,0x01,8);
  _v_writestring(opb,"vorbis", 6);

  /* basic information about the stream */
  oggpack_write(opb,0x00,32);
  oggpack_write(opb,vi->channels,8);
  oggpack_write(opb,vi->rate,32);

  oggpack_write(opb,vi->bitrate_upper,32);
  oggpack_write(opb,vi->bitrate_nominal,32);
  oggpack_write(opb,vi->bitrate_lower,32);

  oggpack_write(opb,ov_ilog(ci->blocksizes[0]-1),4);
  oggpack_write(opb,ov_ilog(ci->blocksizes[1]-1),4);
  oggpack_write(opb,1,1);

  return(0);
}

static int _vorbis_pack_comment(oggpack_buffer *opb,vorbis_comment *vc){
  int bytes = strlen(ENCODE_VENDOR_STRING);

  /* preamble */
  oggpack_write(opb,0x03,8);
  _v_writestring(opb,"vorbis", 6);

  /* vendor */
  oggpack_write(opb,bytes,32);
  _v_writestring(opb,ENCODE_VENDOR_STRING, bytes);

  /* comments */

  oggpack_write(opb,vc->comments,32);
  if(vc->comments){
    int i;
    for(i=0;i<vc->comments;i++){
      if(vc->user_comments[i]){
        oggpack_write(opb,vc->comment_lengths[i],32);
        _v_writestring(opb,vc->user_comments[i], vc->comment_lengths[i]);
      }else{
        oggpack_write(opb,0,32);
      }
    }
  }
  oggpack_write(opb,1,1);

  return(0);
}

static int _vorbis_pack_books(oggpack_buffer *opb,vorbis_info *vi){
  codec_setup_info     *ci=(codec_setup_info*)vi->codec_setup;
  int i;
  if(!ci)return(OV_EFAULT);

  oggpack_write(opb,0x05,8);
  _v_writestring(opb,"vorbis", 6);

  /* books */
  oggpack_write(opb,ci->books-1,8);
  for(i=0;i<ci->books;i++)
    if(vorbis_staticbook_pack(ci->book_param[i],opb))goto err_out;

  /* times; hook placeholders */
  oggpack_write(opb,0,6);
  oggpack_write(opb,0,16);

  /* floors */
  oggpack_write(opb,ci->floors-1,6);
  for(i=0;i<ci->floors;i++){
    oggpack_write(opb,ci->floor_type[i],16);
    if(_floor_P[ci->floor_type[i]]->pack)
      _floor_P[ci->floor_type[i]]->pack(ci->floor_param[i],opb);
    else
      goto err_out;
  }

  /* residues */
  oggpack_write(opb,ci->residues-1,6);
  for(i=0;i<ci->residues;i++){
    oggpack_write(opb,ci->residue_type[i],16);
    _residue_P[ci->residue_type[i]]->pack(ci->residue_param[i],opb);
  }

  /* maps */
  oggpack_write(opb,ci->maps-1,6);
  for(i=0;i<ci->maps;i++){
    oggpack_write(opb,ci->map_type[i],16);
    _mapping_P[ci->map_type[i]]->pack(vi,ci->map_param[i],opb);
  }

  /* modes */
  oggpack_write(opb,ci->modes-1,6);
  for(i=0;i<ci->modes;i++){
    oggpack_write(opb,ci->mode_param[i]->blockflag,1);
    oggpack_write(opb,ci->mode_param[i]->windowtype,16);
    oggpack_write(opb,ci->mode_param[i]->transformtype,16);
    oggpack_write(opb,ci->mode_param[i]->mapping,8);
  }
  oggpack_write(opb,1,1);

  return(0);
err_out:
  return(-1);
}

int vorbis_commentheader_out(vorbis_comment *vc,
                                          ogg_packet *op){

  oggpack_buffer opb;

  oggpack_writeinit(&opb);
  if(_vorbis_pack_comment(&opb,vc)){
    oggpack_writeclear(&opb);
    return OV_EIMPL;
  }

  op->packet = (unsigned char*)_ogg_malloc(oggpack_bytes(&opb));
  memcpy(op->packet, opb.buffer, oggpack_bytes(&opb));

  op->bytes=oggpack_bytes(&opb);
  op->b_o_s=0;
  op->e_o_s=0;
  op->granulepos=0;
  op->packetno=1;

  oggpack_writeclear(&opb);
  return 0;
}

int vorbis_analysis_headerout(vorbis_dsp_state *v,
                              vorbis_comment *vc,
                              ogg_packet *op,
                              ogg_packet *op_comm,
                              ogg_packet *op_code){
  int ret=OV_EIMPL;
  vorbis_info *vi=v->vi;
  oggpack_buffer opb;
  private_state *b=(private_state*)v->backend_state;

  if(!b||vi->channels<=0||vi->channels>256){
    b = NULL;
    ret=OV_EFAULT;
    goto err_out;
  }

  /* first header packet **********************************************/

  oggpack_writeinit(&opb);
  if(_vorbis_pack_info(&opb,vi))goto err_out;

  /* build the packet */
  if(b->header)_ogg_free(b->header);
  b->header=(unsigned char*)_ogg_malloc(oggpack_bytes(&opb));
  memcpy(b->header,opb.buffer,oggpack_bytes(&opb));
  op->packet=b->header;
  op->bytes=oggpack_bytes(&opb);
  op->b_o_s=1;
  op->e_o_s=0;
  op->granulepos=0;
  op->packetno=0;

  /* second header packet (comments) **********************************/

  oggpack_reset(&opb);
  if(_vorbis_pack_comment(&opb,vc))goto err_out;

  if(b->header1)_ogg_free(b->header1);
  b->header1=(unsigned char*)_ogg_malloc(oggpack_bytes(&opb));
  memcpy(b->header1,opb.buffer,oggpack_bytes(&opb));
  op_comm->packet=b->header1;
  op_comm->bytes=oggpack_bytes(&opb);
  op_comm->b_o_s=0;
  op_comm->e_o_s=0;
  op_comm->granulepos=0;
  op_comm->packetno=1;

  /* third header packet (modes/codebooks) ****************************/

  oggpack_reset(&opb);
  if(_vorbis_pack_books(&opb,vi))goto err_out;

  if(b->header2)_ogg_free(b->header2);
  b->header2=(unsigned char*)_ogg_malloc(oggpack_bytes(&opb));
  memcpy(b->header2,opb.buffer,oggpack_bytes(&opb));
  op_code->packet=b->header2;
  op_code->bytes=oggpack_bytes(&opb);
  op_code->b_o_s=0;
  op_code->e_o_s=0;
  op_code->granulepos=0;
  op_code->packetno=2;

  oggpack_writeclear(&opb);
  return(0);
 err_out:
  memset(op,0,sizeof(*op));
  memset(op_comm,0,sizeof(*op_comm));
  memset(op_code,0,sizeof(*op_code));

  if(b){
    if(vi->channels>0)oggpack_writeclear(&opb);
    if(b->header)_ogg_free(b->header);
    if(b->header1)_ogg_free(b->header1);
    if(b->header2)_ogg_free(b->header2);
    b->header=NULL;
    b->header1=NULL;
    b->header2=NULL;
  }
  return(ret);
}

double vorbis_granule_time(vorbis_dsp_state *v,ogg_int64_t granulepos){
  if(granulepos == -1) return -1;

  /* We're not guaranteed a 64 bit unsigned type everywhere, so we
     have to put the unsigned granpo in a signed type. */
  if(granulepos>=0){
    return((double)granulepos/v->vi->rate);
  }else{
    ogg_int64_t granuleoff=0xffffffff;
    granuleoff<<=31;
    granuleoff|=0x7ffffffff;
    return(((double)granulepos+2+granuleoff+granuleoff)/v->vi->rate);
  }
}

const char *vorbis_version_string(void){
  return GENERAL_VENDOR_STRING;
}
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2015             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: floor backend 1 implementation

 ********************************************************************/

#include <stdlib.h>
#include <string.h>
#include <math.h>
/*#include <ogg/ogg.h>*/
/*#include "vorbis/codec.h"*/
/*#include "codec_internal.h"*/
/*#include "registry.h"*/
/*#include "codebook.h"*/
/*#include "misc.h"*/
/*#include "scales.h"*/

#include <stdio.h>

#define floor1_rangedB 140 /* floor 1 fixed at -140dB to 0dB range */

typedef struct lsfit_acc{
  int x0;
  int x1;

  int xa;
  int ya;
  int x2a;
  int y2a;
  int xya;
  int an;

  int xb;
  int yb;
  int x2b;
  int y2b;
  int xyb;
  int bn;
} lsfit_acc;

/***********************************************/

static void floor1_free_info(vorbis_info_floor *i){
  vorbis_info_floor1 *info=(vorbis_info_floor1 *)i;
  if(info){
    memset(info,0,sizeof(*info));
    _ogg_free(info);
  }
}

static void floor1_free_look(vorbis_look_floor *i){
  vorbis_look_floor1 *look=(vorbis_look_floor1 *)i;
  if(look){
    /*fprintf(stderr,"floor 1 bit usage %f:%f (%f total)\n",
            (float)look->phrasebits/look->frames,
            (float)look->postbits/look->frames,
            (float)(look->postbits+look->phrasebits)/look->frames);*/

    memset(look,0,sizeof(*look));
    _ogg_free(look);
  }
}

static void floor1_pack (vorbis_info_floor *i,oggpack_buffer *opb){
  vorbis_info_floor1 *info=(vorbis_info_floor1 *)i;
  int j,k;
  int count=0;
  int rangebits;
  int maxposit=info->postlist[1];
  int maxclass=-1;

  /* save out partitions */
  oggpack_write(opb,info->partitions,5); /* only 0 to 31 legal */
  for(j=0;j<info->partitions;j++){
    oggpack_write(opb,info->partitionclass[j],4); /* only 0 to 15 legal */
    if(maxclass<info->partitionclass[j])maxclass=info->partitionclass[j];
  }

  /* save out partition classes */
  for(j=0;j<maxclass+1;j++){
    oggpack_write(opb,info->class_dim[j]-1,3); /* 1 to 8 */
    oggpack_write(opb,info->class_subs[j],2); /* 0 to 3 */
    if(info->class_subs[j])oggpack_write(opb,info->class_book[j],8);
    for(k=0;k<(1<<info->class_subs[j]);k++)
      oggpack_write(opb,info->class_subbook[j][k]+1,8);
  }

  /* save out the post list */
  oggpack_write(opb,info->mult-1,2);     /* only 1,2,3,4 legal now */
  /* maxposit cannot legally be less than 1; this is encode-side, we
     can assume our setup is OK */
  oggpack_write(opb,ov_ilog(maxposit-1),4);
  rangebits=ov_ilog(maxposit-1);

  for(j=0,k=0;j<info->partitions;j++){
    count+=info->class_dim[info->partitionclass[j]];
    for(;k<count;k++)
      oggpack_write(opb,info->postlist[k+2],rangebits);
  }
}

static int icomp(const void *a,const void *b){
  return(**(int **)a-**(int **)b);
}

static vorbis_info_floor *floor1_unpack (vorbis_info *vi,oggpack_buffer *opb){
  codec_setup_info     *ci=(codec_setup_info*)vi->codec_setup;
  int j,k,count=0,maxclass=-1,rangebits;

  vorbis_info_floor1 *info=(vorbis_info_floor1*)_ogg_calloc(1,sizeof(*info));
  /* read partitions */
  info->partitions=oggpack_read(opb,5); /* only 0 to 31 legal */
  for(j=0;j<info->partitions;j++){
    info->partitionclass[j]=oggpack_read(opb,4); /* only 0 to 15 legal */
    if(info->partitionclass[j]<0)goto err_out;
    if(maxclass<info->partitionclass[j])maxclass=info->partitionclass[j];
  }

  /* read partition classes */
  for(j=0;j<maxclass+1;j++){
    info->class_dim[j]=oggpack_read(opb,3)+1; /* 1 to 8 */
    info->class_subs[j]=oggpack_read(opb,2); /* 0,1,2,3 bits */
    if(info->class_subs[j]<0)
      goto err_out;
    if(info->class_subs[j])info->class_book[j]=oggpack_read(opb,8);
    if(info->class_book[j]<0 || info->class_book[j]>=ci->books)
      goto err_out;
    for(k=0;k<(1<<info->class_subs[j]);k++){
      info->class_subbook[j][k]=oggpack_read(opb,8)-1;
      if(info->class_subbook[j][k]<-1 || info->class_subbook[j][k]>=ci->books)
        goto err_out;
    }
  }

  /* read the post list */
  info->mult=oggpack_read(opb,2)+1;     /* only 1,2,3,4 legal now */
  rangebits=oggpack_read(opb,4);
  if(rangebits<0)goto err_out;

  for(j=0,k=0;j<info->partitions;j++){
    count+=info->class_dim[info->partitionclass[j]];
    if(count>VIF_POSIT) goto err_out;
    for(;k<count;k++){
      int t=info->postlist[k+2]=oggpack_read(opb,rangebits);
      if(t<0 || t>=(1<<rangebits))
        goto err_out;
    }
  }
  info->postlist[0]=0;
  info->postlist[1]=1<<rangebits;

  /* don't allow repeated values in post list as they'd result in
     zero-length segments */
  {
    int *sortpointer[VIF_POSIT+2];
    for(j=0;j<count+2;j++)sortpointer[j]=info->postlist+j;
    qsort(sortpointer,count+2,sizeof(*sortpointer),icomp);

    for(j=1;j<count+2;j++)
      if(*sortpointer[j-1]==*sortpointer[j])goto err_out;
  }

  return(info);

 err_out:
  floor1_free_info(info);
  return(NULL);
}

static vorbis_look_floor *floor1_look(vorbis_dsp_state *vd,
                                      vorbis_info_floor *in){

  int *sortpointer[VIF_POSIT+2];
  vorbis_info_floor1 *info=(vorbis_info_floor1 *)in;
  vorbis_look_floor1 *look=(vorbis_look_floor1*)_ogg_calloc(1,sizeof(*look));
  int i,j,n=0;

  (void)vd;

  look->vi=info;
  look->n=info->postlist[1];

  /* we drop each position value in-between already decoded values,
     and use linear interpolation to predict each new value past the
     edges.  The positions are read in the order of the position
     list... we precompute the bounding positions in the lookup.  Of
     course, the neighbors can change (if a position is declined), but
     this is an initial mapping */

  for(i=0;i<info->partitions;i++)n+=info->class_dim[info->partitionclass[i]];
  n+=2;
  look->posts=n;

  /* also store a sorted position index */
  for(i=0;i<n;i++)sortpointer[i]=info->postlist+i;
  qsort(sortpointer,n,sizeof(*sortpointer),icomp);

  /* points from sort order back to range number */
  for(i=0;i<n;i++)look->forward_index[i]=sortpointer[i]-info->postlist;
  /* points from range order to sorted position */
  for(i=0;i<n;i++)look->reverse_index[look->forward_index[i]]=i;
  /* we actually need the post values too */
  for(i=0;i<n;i++)look->sorted_index[i]=info->postlist[look->forward_index[i]];

  /* quantize values to multiplier spec */
  switch(info->mult){
  case 1: /* 1024 -> 256 */
    look->quant_q=256;
    break;
  case 2: /* 1024 -> 128 */
    look->quant_q=128;
    break;
  case 3: /* 1024 -> 86 */
    look->quant_q=86;
    break;
  case 4: /* 1024 -> 64 */
    look->quant_q=64;
    break;
  }

  /* discover our neighbors for decode where we don't use fit flags
     (that would push the neighbors outward) */
  for(i=0;i<n-2;i++){
    int lo=0;
    int hi=1;
    int lx=0;
    int hx=look->n;
    int currentx=info->postlist[i+2];
    for(j=0;j<i+2;j++){
      int x=info->postlist[j];
      if(x>lx && x<currentx){
        lo=j;
        lx=x;
      }
      if(x<hx && x>currentx){
        hi=j;
        hx=x;
      }
    }
    look->loneighbor[i]=lo;
    look->hineighbor[i]=hi;
  }

  return(look);
}

static int render_point(int x0,int x1,int y0,int y1,int x){
  y0&=0x7fff; /* mask off flag */
  y1&=0x7fff;

  {
    int dy=y1-y0;
    int adx=x1-x0;
    int ady=abs(dy);
    int err=ady*(x-x0);

    int off=err/adx;
    if(dy<0)return(y0-off);
    return(y0+off);
  }
}

static int vorbis_dBquant(const float *x){
  int i= *x*7.3142857f+1023.5f;
  if(i>1023)return(1023);
  if(i<0)return(0);
  return i;
}

static const float FLOOR1_fromdB_LOOKUP[256]={
  1.0649863e-07F, 1.1341951e-07F, 1.2079015e-07F, 1.2863978e-07F,
  1.3699951e-07F, 1.4590251e-07F, 1.5538408e-07F, 1.6548181e-07F,
  1.7623575e-07F, 1.8768855e-07F, 1.9988561e-07F, 2.128753e-07F,
  2.2670913e-07F, 2.4144197e-07F, 2.5713223e-07F, 2.7384213e-07F,
  2.9163793e-07F, 3.1059021e-07F, 3.3077411e-07F, 3.5226968e-07F,
  3.7516214e-07F, 3.9954229e-07F, 4.2550680e-07F, 4.5315863e-07F,
  4.8260743e-07F, 5.1396998e-07F, 5.4737065e-07F, 5.8294187e-07F,
  6.2082472e-07F, 6.6116941e-07F, 7.0413592e-07F, 7.4989464e-07F,
  7.9862701e-07F, 8.5052630e-07F, 9.0579828e-07F, 9.6466216e-07F,
  1.0273513e-06F, 1.0941144e-06F, 1.1652161e-06F, 1.2409384e-06F,
  1.3215816e-06F, 1.4074654e-06F, 1.4989305e-06F, 1.5963394e-06F,
  1.7000785e-06F, 1.8105592e-06F, 1.9282195e-06F, 2.0535261e-06F,
  2.1869758e-06F, 2.3290978e-06F, 2.4804557e-06F, 2.6416497e-06F,
  2.8133190e-06F, 2.9961443e-06F, 3.1908506e-06F, 3.3982101e-06F,
  3.6190449e-06F, 3.8542308e-06F, 4.1047004e-06F, 4.3714470e-06F,
  4.6555282e-06F, 4.9580707e-06F, 5.2802740e-06F, 5.6234160e-06F,
  5.9888572e-06F, 6.3780469e-06F, 6.7925283e-06F, 7.2339451e-06F,
  7.7040476e-06F, 8.2047000e-06F, 8.7378876e-06F, 9.3057248e-06F,
  9.9104632e-06F, 1.0554501e-05F, 1.1240392e-05F, 1.1970856e-05F,
  1.2748789e-05F, 1.3577278e-05F, 1.4459606e-05F, 1.5399272e-05F,
  1.6400004e-05F, 1.7465768e-05F, 1.8600792e-05F, 1.9809576e-05F,
  2.1096914e-05F, 2.2467911e-05F, 2.3928002e-05F, 2.5482978e-05F,
  2.7139006e-05F, 2.8902651e-05F, 3.0780908e-05F, 3.2781225e-05F,
  3.4911534e-05F, 3.7180282e-05F, 3.9596466e-05F, 4.2169667e-05F,
  4.4910090e-05F, 4.7828601e-05F, 5.0936773e-05F, 5.4246931e-05F,
  5.7772202e-05F, 6.1526565e-05F, 6.5524908e-05F, 6.9783085e-05F,
  7.4317983e-05F, 7.9147585e-05F, 8.4291040e-05F, 8.9768747e-05F,
  9.5602426e-05F, 0.00010181521F, 0.00010843174F, 0.00011547824F,
  0.00012298267F, 0.00013097477F, 0.00013948625F, 0.00014855085F,
  0.00015820453F, 0.00016848555F, 0.00017943469F, 0.00019109536F,
  0.00020351382F, 0.00021673929F, 0.00023082423F, 0.00024582449F,
  0.00026179955F, 0.00027881276F, 0.00029693158F, 0.00031622787F,
  0.00033677814F, 0.00035866388F, 0.00038197188F, 0.00040679456F,
  0.00043323036F, 0.00046138411F, 0.00049136745F, 0.00052329927F,
  0.00055730621F, 0.00059352311F, 0.00063209358F, 0.00067317058F,
  0.00071691700F, 0.00076350630F, 0.00081312324F, 0.00086596457F,
  0.00092223983F, 0.00098217216F, 0.0010459992F, 0.0011139742F,
  0.0011863665F, 0.0012634633F, 0.0013455702F, 0.0014330129F,
  0.0015261382F, 0.0016253153F, 0.0017309374F, 0.0018434235F,
  0.0019632195F, 0.0020908006F, 0.0022266726F, 0.0023713743F,
  0.0025254795F, 0.0026895994F, 0.0028643847F, 0.0030505286F,
  0.0032487691F, 0.0034598925F, 0.0036847358F, 0.0039241906F,
  0.0041792066F, 0.0044507950F, 0.0047400328F, 0.0050480668F,
  0.0053761186F, 0.0057254891F, 0.0060975636F, 0.0064938176F,
  0.0069158225F, 0.0073652516F, 0.0078438871F, 0.0083536271F,
  0.0088964928F, 0.009474637F, 0.010090352F, 0.010746080F,
  0.011444421F, 0.012188144F, 0.012980198F, 0.013823725F,
  0.014722068F, 0.015678791F, 0.016697687F, 0.017782797F,
  0.018938423F, 0.020169149F, 0.021479854F, 0.022875735F,
  0.024362330F, 0.025945531F, 0.027631618F, 0.029427276F,
  0.031339626F, 0.033376252F, 0.035545228F, 0.037855157F,
  0.040315199F, 0.042935108F, 0.045725273F, 0.048696758F,
  0.051861348F, 0.055231591F, 0.058820850F, 0.062643361F,
  0.066714279F, 0.071049749F, 0.075666962F, 0.080584227F,
  0.085821044F, 0.091398179F, 0.097337747F, 0.10366330F,
  0.11039993F, 0.11757434F, 0.12521498F, 0.13335215F,
  0.14201813F, 0.15124727F, 0.16107617F, 0.17154380F,
  0.18269168F, 0.19456402F, 0.20720788F, 0.22067342F,
  0.23501402F, 0.25028656F, 0.26655159F, 0.28387361F,
  0.30232132F, 0.32196786F, 0.34289114F, 0.36517414F,
  0.38890521F, 0.41417847F, 0.44109412F, 0.46975890F,
  0.50028648F, 0.53279791F, 0.56742212F, 0.60429640F,
  0.64356699F, 0.68538959F, 0.72993007F, 0.77736504F,
  0.82788260F, 0.88168307F, 0.9389798F, 1.F,
};

static void render_line(int n, int x0,int x1,int y0,int y1,float *d){
  int dy=y1-y0;
  int adx=x1-x0;
  int ady=abs(dy);
  int base=dy/adx;
  int sy=(dy<0?base-1:base+1);
  int x=x0;
  int y=y0;
  int err=0;

  ady-=abs(base*adx);

  if(n>x1)n=x1;

  if(x<n)
    d[x]*=FLOOR1_fromdB_LOOKUP[y];

  while(++x<n){
    err=err+ady;
    if(err>=adx){
      err-=adx;
      y+=sy;
    }else{
      y+=base;
    }
    d[x]*=FLOOR1_fromdB_LOOKUP[y];
  }
}

static void render_line0(int n, int x0,int x1,int y0,int y1,int *d){
  int dy=y1-y0;
  int adx=x1-x0;
  int ady=abs(dy);
  int base=dy/adx;
  int sy=(dy<0?base-1:base+1);
  int x=x0;
  int y=y0;
  int err=0;

  ady-=abs(base*adx);

  if(n>x1)n=x1;

  if(x<n)
    d[x]=y;

  while(++x<n){
    err=err+ady;
    if(err>=adx){
      err-=adx;
      y+=sy;
    }else{
      y+=base;
    }
    d[x]=y;
  }
}

/* the floor has already been filtered to only include relevant sections */
static int accumulate_fit(const float *flr,const float *mdct,
                          int x0, int x1,lsfit_acc *a,
                          int n,vorbis_info_floor1 *info){
  long i;

  int xa=0,ya=0,x2a=0,y2a=0,xya=0,na=0, xb=0,yb=0,x2b=0,y2b=0,xyb=0,nb=0;

  memset(a,0,sizeof(*a));
  a->x0=x0;
  a->x1=x1;
  if(x1>=n)x1=n-1;

  for(i=x0;i<=x1;i++){
    int quantized=vorbis_dBquant(flr+i);
    if(quantized){
      if(mdct[i]+info->twofitatten>=flr[i]){
        xa  += i;
        ya  += quantized;
        x2a += i*i;
        y2a += quantized*quantized;
        xya += i*quantized;
        na++;
      }else{
        xb  += i;
        yb  += quantized;
        x2b += i*i;
        y2b += quantized*quantized;
        xyb += i*quantized;
        nb++;
      }
    }
  }

  a->xa=xa;
  a->ya=ya;
  a->x2a=x2a;
  a->y2a=y2a;
  a->xya=xya;
  a->an=na;

  a->xb=xb;
  a->yb=yb;
  a->x2b=x2b;
  a->y2b=y2b;
  a->xyb=xyb;
  a->bn=nb;

  return(na);
}

static int fit_line(lsfit_acc *a,int fits,int *y0,int *y1,
                    vorbis_info_floor1 *info){
  double xb=0,yb=0,x2b=0,y2b=0,xyb=0,bn=0;
  int i;
  int x0=a[0].x0;
  int x1=a[fits-1].x1;

  for(i=0;i<fits;i++){
    double weight = (a[i].bn+a[i].an)*info->twofitweight/(a[i].an+1)+1.;

    xb+=a[i].xb + a[i].xa * weight;
    yb+=a[i].yb + a[i].ya * weight;
    x2b+=a[i].x2b + a[i].x2a * weight;
    y2b+=a[i].y2b + a[i].y2a * weight;
    xyb+=a[i].xyb + a[i].xya * weight;
    bn+=a[i].bn + a[i].an * weight;
  }

  if(*y0>=0){
    xb+=   x0;
    yb+=  *y0;
    x2b+=  x0 *  x0;
    y2b+= *y0 * *y0;
    xyb+= *y0 *  x0;
    bn++;
  }

  if(*y1>=0){
    xb+=   x1;
    yb+=  *y1;
    x2b+=  x1 *  x1;
    y2b+= *y1 * *y1;
    xyb+= *y1 *  x1;
    bn++;
  }

  {
    double denom=(bn*x2b-xb*xb);

    if(denom>0.){
      double a=(yb*x2b-xyb*xb)/denom;
      double b=(bn*xyb-xb*yb)/denom;
      *y0=rint(a+b*x0);
      *y1=rint(a+b*x1);

      /* limit to our range! */
      if(*y0>1023)*y0=1023;
      if(*y1>1023)*y1=1023;
      if(*y0<0)*y0=0;
      if(*y1<0)*y1=0;

      return 0;
    }else{
      *y0=0;
      *y1=0;
      return 1;
    }
  }
}

static int inspect_error(int x0,int x1,int y0,int y1,const float *mask,
                         const float *mdct,
                         vorbis_info_floor1 *info){
  int dy=y1-y0;
  int adx=x1-x0;
  int ady=abs(dy);
  int base=dy/adx;
  int sy=(dy<0?base-1:base+1);
  int x=x0;
  int y=y0;
  int err=0;
  int val=vorbis_dBquant(mask+x);
  int mse=0;
  int n=0;

  ady-=abs(base*adx);

  mse=(y-val);
  mse*=mse;
  n++;
  if(mdct[x]+info->twofitatten>=mask[x]){
    if(y+info->maxover<val)return(1);
    if(y-info->maxunder>val)return(1);
  }

  while(++x<x1){
    err=err+ady;
    if(err>=adx){
      err-=adx;
      y+=sy;
    }else{
      y+=base;
    }

    val=vorbis_dBquant(mask+x);
    mse+=((y-val)*(y-val));
    n++;
    if(mdct[x]+info->twofitatten>=mask[x]){
      if(val){
        if(y+info->maxover<val)return(1);
        if(y-info->maxunder>val)return(1);
      }
    }
  }

  if(info->maxover*info->maxover/n>info->maxerr)return(0);
  if(info->maxunder*info->maxunder/n>info->maxerr)return(0);
  if(mse/n>info->maxerr)return(1);
  return(0);
}

static int post_Y(int *A,int *B,int pos){
  if(A[pos]<0)
    return B[pos];
  if(B[pos]<0)
    return A[pos];

  return (A[pos]+B[pos])>>1;
}

int *floor1_fit(vorbis_block *vb,vorbis_look_floor1 *look,
                          const float *logmdct,   /* in */
                          const float *logmask){
  long i,j;
  vorbis_info_floor1 *info=look->vi;
  long n=look->n;
  long posts=look->posts;
  long nonzero=0;
  lsfit_acc fits[VIF_POSIT+1];
  int fit_valueA[VIF_POSIT+2]; /* index by range list position */
  int fit_valueB[VIF_POSIT+2]; /* index by range list position */

  int loneighbor[VIF_POSIT+2]; /* sorted index of range list position (+2) */
  int hineighbor[VIF_POSIT+2];
  int *output=NULL;
  int memo[VIF_POSIT+2];

  for(i=0;i<posts;i++)fit_valueA[i]=-200; /* mark all unused */
  for(i=0;i<posts;i++)fit_valueB[i]=-200; /* mark all unused */
  for(i=0;i<posts;i++)loneighbor[i]=0; /* 0 for the implicit 0 post */
  for(i=0;i<posts;i++)hineighbor[i]=1; /* 1 for the implicit post at n */
  for(i=0;i<posts;i++)memo[i]=-1;      /* no neighbor yet */

  /* quantize the relevant floor points and collect them into line fit
     structures (one per minimal division) at the same time */
  if(posts==0){
    nonzero+=accumulate_fit(logmask,logmdct,0,n,fits,n,info);
  }else{
    for(i=0;i<posts-1;i++)
      nonzero+=accumulate_fit(logmask,logmdct,look->sorted_index[i],
                              look->sorted_index[i+1],fits+i,
                              n,info);
  }

  if(nonzero){
    /* start by fitting the implicit base case.... */
    int y0=-200;
    int y1=-200;
    fit_line(fits,posts-1,&y0,&y1,info);

    fit_valueA[0]=y0;
    fit_valueB[0]=y0;
    fit_valueB[1]=y1;
    fit_valueA[1]=y1;

    /* Non degenerate case */
    /* start progressive splitting.  This is a greedy, non-optimal
       algorithm, but simple and close enough to the best
       answer. */
    for(i=2;i<posts;i++){
      int sortpos=look->reverse_index[i];
      int ln=loneighbor[sortpos];
      int hn=hineighbor[sortpos];

      /* eliminate repeat searches of a particular range with a memo */
      if(memo[ln]!=hn){
        /* haven't performed this error search yet */
        int lsortpos=look->reverse_index[ln];
        int hsortpos=look->reverse_index[hn];
        memo[ln]=hn;

        {
          /* A note: we want to bound/minimize *local*, not global, error */
          int lx=info->postlist[ln];
          int hx=info->postlist[hn];
          int ly=post_Y(fit_valueA,fit_valueB,ln);
          int hy=post_Y(fit_valueA,fit_valueB,hn);

          if(ly==-1 || hy==-1){
            exit(1);
          }

          if(inspect_error(lx,hx,ly,hy,logmask,logmdct,info)){
            /* outside error bounds/begin search area.  Split it. */
            int ly0=-200;
            int ly1=-200;
            int hy0=-200;
            int hy1=-200;
            int ret0=fit_line(fits+lsortpos,sortpos-lsortpos,&ly0,&ly1,info);
            int ret1=fit_line(fits+sortpos,hsortpos-sortpos,&hy0,&hy1,info);

            if(ret0){
              ly0=ly;
              ly1=hy0;
            }
            if(ret1){
              hy0=ly1;
              hy1=hy;
            }

            if(ret0 && ret1){
              fit_valueA[i]=-200;
              fit_valueB[i]=-200;
            }else{
              /* store new edge values */
              fit_valueB[ln]=ly0;
              if(ln==0)fit_valueA[ln]=ly0;
              fit_valueA[i]=ly1;
              fit_valueB[i]=hy0;
              fit_valueA[hn]=hy1;
              if(hn==1)fit_valueB[hn]=hy1;

              if(ly1>=0 || hy0>=0){
                /* store new neighbor values */
                for(j=sortpos-1;j>=0;j--)
                  if(hineighbor[j]==hn)
                    hineighbor[j]=i;
                  else
                    break;
                for(j=sortpos+1;j<posts;j++)
                  if(loneighbor[j]==ln)
                    loneighbor[j]=i;
                  else
                    break;
              }
            }
          }else{
            fit_valueA[i]=-200;
            fit_valueB[i]=-200;
          }
        }
      }
    }

    output=(int*)_vorbis_block_alloc(vb,sizeof(*output)*posts);

    output[0]=post_Y(fit_valueA,fit_valueB,0);
    output[1]=post_Y(fit_valueA,fit_valueB,1);

    /* fill in posts marked as not using a fit; we will zero
       back out to 'unused' when encoding them so long as curve
       interpolation doesn't force them into use */
    for(i=2;i<posts;i++){
      int ln=look->loneighbor[i-2];
      int hn=look->hineighbor[i-2];
      int x0=info->postlist[ln];
      int x1=info->postlist[hn];
      int y0=output[ln];
      int y1=output[hn];

      int predicted=render_point(x0,x1,y0,y1,info->postlist[i]);
      int vx=post_Y(fit_valueA,fit_valueB,i);

      if(vx>=0 && predicted!=vx){
        output[i]=vx;
      }else{
        output[i]= predicted|0x8000;
      }
    }
  }

  return(output);

}

int *floor1_interpolate_fit(vorbis_block *vb,vorbis_look_floor1 *look,
                          int *A,int *B,
                          int del){

  long i;
  long posts=look->posts;
  int *output=NULL;

  if(A && B){
    output= (int*)_vorbis_block_alloc(vb,sizeof(*output)*posts);

    /* overly simpleminded--- look again post 1.2 */
    for(i=0;i<posts;i++){
      output[i]=((65536-del)*(A[i]&0x7fff)+del*(B[i]&0x7fff)+32768)>>16;
      if(A[i]&0x8000 && B[i]&0x8000)output[i]|=0x8000;
    }
  }

  return(output);
}


int floor1_encode(oggpack_buffer *opb,vorbis_block *vb,
                  vorbis_look_floor1 *look,
                  int *post,int *ilogmask){

  long i,j;
  vorbis_info_floor1 *info=look->vi;
  long posts=look->posts;
  codec_setup_info *ci=(codec_setup_info*)vb->vd->vi->codec_setup;
  int out[VIF_POSIT+2];
  static_codebook **sbooks=ci->book_param;
  codebook *books=ci->fullbooks;

  /* quantize values to multiplier spec */
  if(post){
    for(i=0;i<posts;i++){
      int val=post[i]&0x7fff;
      switch(info->mult){
      case 1: /* 1024 -> 256 */
        val>>=2;
        break;
      case 2: /* 1024 -> 128 */
        val>>=3;
        break;
      case 3: /* 1024 -> 86 */
        val/=12;
        break;
      case 4: /* 1024 -> 64 */
        val>>=4;
        break;
      }
      post[i]=val | (post[i]&0x8000);
    }

    out[0]=post[0];
    out[1]=post[1];

    /* find prediction values for each post and subtract them */
    for(i=2;i<posts;i++){
      int ln=look->loneighbor[i-2];
      int hn=look->hineighbor[i-2];
      int x0=info->postlist[ln];
      int x1=info->postlist[hn];
      int y0=post[ln];
      int y1=post[hn];

      int predicted=render_point(x0,x1,y0,y1,info->postlist[i]);

      if((post[i]&0x8000) || (predicted==post[i])){
        post[i]=predicted|0x8000; /* in case there was roundoff jitter
                                     in interpolation */
        out[i]=0;
      }else{
        int headroom=(look->quant_q-predicted<predicted?
                      look->quant_q-predicted:predicted);

        int val=post[i]-predicted;

        /* at this point the 'deviation' value is in the range +/- max
           range, but the real, unique range can always be mapped to
           only [0-maxrange).  So we want to wrap the deviation into
           this limited range, but do it in the way that least screws
           an essentially gaussian probability distribution. */

        if(val<0)
          if(val<-headroom)
            val=headroom-val-1;
          else
            val=-1-(val<<1);
        else
          if(val>=headroom)
            val= val+headroom;
          else
            val<<=1;

        out[i]=val;
        post[ln]&=0x7fff;
        post[hn]&=0x7fff;
      }
    }

    /* we have everything we need. pack it out */
    /* mark nontrivial floor */
    oggpack_write(opb,1,1);

    /* beginning/end post */
    look->frames++;
    look->postbits+=ov_ilog(look->quant_q-1)*2;
    oggpack_write(opb,out[0],ov_ilog(look->quant_q-1));
    oggpack_write(opb,out[1],ov_ilog(look->quant_q-1));


    /* partition by partition */
    for(i=0,j=2;i<info->partitions;i++){
      int class_=info->partitionclass[i];
      int cdim=info->class_dim[class_];
      int csubbits=info->class_subs[class_];
      int csub=1<<csubbits;
      int bookas[8]={0,0,0,0,0,0,0,0};
      int cval=0;
      int cshift=0;
      int k,l;

      /* generate the partition's first stage cascade value */
      if(csubbits){
        int maxval[8]={0,0,0,0,0,0,0,0}; /* gcc's static analysis
                                            issues a warning without
                                            initialization */
        for(k=0;k<csub;k++){
          int booknum=info->class_subbook[class_][k];
          if(booknum<0){
            maxval[k]=1;
          }else{
            maxval[k]=sbooks[info->class_subbook[class_][k]]->entries;
          }
        }
        for(k=0;k<cdim;k++){
          for(l=0;l<csub;l++){
            int val=out[j+k];
            if(val<maxval[l]){
              bookas[k]=l;
              break;
            }
          }
          cval|= bookas[k]<<cshift;
          cshift+=csubbits;
        }
        /* write it */
        look->phrasebits+=
          vorbis_book_encode(books+info->class_book[class_],cval,opb);

#ifdef TRAIN_FLOOR1
        {
          FILE *of;
          char buffer[80];
          sprintf(buffer,"line_%dx%ld_class%d.vqd",
                  vb->pcmend/2,posts-2, class_);
          of=fopen(buffer,"a");
          fprintf(of,"%d\n",cval);
          fclose(of);
        }
#endif
      }

      /* write post values */
      for(k=0;k<cdim;k++){
        int book=info->class_subbook[class_][bookas[k]];
        if(book>=0){
          /* hack to allow training with 'bad' books */
          if(out[j+k]<(books+book)->entries)
            look->postbits+=vorbis_book_encode(books+book,
                                               out[j+k],opb);
          /*else
            fprintf(stderr,"+!");*/

#ifdef TRAIN_FLOOR1
          {
            FILE *of;
            char buffer[80];
            sprintf(buffer,"line_%dx%ld_%dsub%d.vqd",
                    vb->pcmend/2,posts-2, class_,bookas[k]);
            of=fopen(buffer,"a");
            fprintf(of,"%d\n",out[j+k]);
            fclose(of);
          }
#endif
        }
      }
      j+=cdim;
    }

    {
      /* generate quantized floor equivalent to what we'd unpack in decode */
      /* render the lines */
      int hx=0;
      int lx=0;
      int ly=post[0]*info->mult;
      int n=ci->blocksizes[vb->W]/2;

      for(j=1;j<look->posts;j++){
        int current=look->forward_index[j];
        int hy=post[current]&0x7fff;
        if(hy==post[current]){

          hy*=info->mult;
          hx=info->postlist[current];

          render_line0(n,lx,hx,ly,hy,ilogmask);

          lx=hx;
          ly=hy;
        }
      }
      for(j=hx;j<vb->pcmend/2;j++)ilogmask[j]=ly; /* be certain */
      return(1);
    }
  }else{
    oggpack_write(opb,0,1);
    memset(ilogmask,0,vb->pcmend/2*sizeof(*ilogmask));
    return(0);
  }
}

static void *floor1_inverse1(vorbis_block *vb,vorbis_look_floor *in){
  vorbis_look_floor1 *look=(vorbis_look_floor1 *)in;
  vorbis_info_floor1 *info=look->vi;
  codec_setup_info   *ci=(codec_setup_info*)vb->vd->vi->codec_setup;

  int i,j,k;
  codebook *books=ci->fullbooks;

  /* unpack wrapped/predicted values from stream */
  if(oggpack_read(&vb->opb,1)==1){
    int *fit_value=(int*)_vorbis_block_alloc(vb,(look->posts)*sizeof(*fit_value));

    fit_value[0]=oggpack_read(&vb->opb,ov_ilog(look->quant_q-1));
    fit_value[1]=oggpack_read(&vb->opb,ov_ilog(look->quant_q-1));

    /* partition by partition */
    for(i=0,j=2;i<info->partitions;i++){
      int class_ =info->partitionclass[i];
      int cdim=info->class_dim[class_];
      int csubbits=info->class_subs[class_];
      int csub=1<<csubbits;
      int cval=0;

      /* decode the partition's first stage cascade value */
      if(csubbits){
        cval=vorbis_book_decode(books+info->class_book[class_],&vb->opb);

        if(cval==-1)goto eop;
      }

      for(k=0;k<cdim;k++){
        int book=info->class_subbook[class_][cval&(csub-1)];
        cval>>=csubbits;
        if(book>=0){
          if((fit_value[j+k]=vorbis_book_decode(books+book,&vb->opb))==-1)
            goto eop;
        }else{
          fit_value[j+k]=0;
        }
      }
      j+=cdim;
    }

    /* unwrap positive values and reconsitute via linear interpolation */
    for(i=2;i<look->posts;i++){
      int predicted=render_point(info->postlist[look->loneighbor[i-2]],
                                 info->postlist[look->hineighbor[i-2]],
                                 fit_value[look->loneighbor[i-2]],
                                 fit_value[look->hineighbor[i-2]],
                                 info->postlist[i]);
      int hiroom=look->quant_q-predicted;
      int loroom=predicted;
      int room=(hiroom<loroom?hiroom:loroom)<<1;
      int val=fit_value[i];

      if(val){
        if(val>=room){
          if(hiroom>loroom){
            val = val-loroom;
          }else{
            val = -1-(val-hiroom);
          }
        }else{
          if(val&1){
            val= -((val+1)>>1);
          }else{
            val>>=1;
          }
        }

        fit_value[i]=(val+predicted)&0x7fff;
        fit_value[look->loneighbor[i-2]]&=0x7fff;
        fit_value[look->hineighbor[i-2]]&=0x7fff;

      }else{
        fit_value[i]=predicted|0x8000;
      }

    }

    return(fit_value);
  }
 eop:
  return(NULL);
}

static int floor1_inverse2(vorbis_block *vb,vorbis_look_floor *in,void *memo,
                          float *out){
  vorbis_look_floor1 *look=(vorbis_look_floor1 *)in;
  vorbis_info_floor1 *info=look->vi;

  codec_setup_info   *ci=(codec_setup_info*)vb->vd->vi->codec_setup;
  int                  n=ci->blocksizes[vb->W]/2;
  int j;

  if(memo){
    /* render the lines */
    int *fit_value=(int *)memo;
    int hx=0;
    int lx=0;
    int ly=fit_value[0]*info->mult;
    /* guard lookup against out-of-range values */
    ly=(ly<0?0:ly>255?255:ly);

    for(j=1;j<look->posts;j++){
      int current=look->forward_index[j];
      int hy=fit_value[current]&0x7fff;
      if(hy==fit_value[current]){

        hx=info->postlist[current];
        hy*=info->mult;
        /* guard lookup against out-of-range values */
        hy=(hy<0?0:hy>255?255:hy);

        render_line(n,lx,hx,ly,hy,out);

        lx=hx;
        ly=hy;
      }
    }
    for(j=hx;j<n;j++)out[j]*=FLOOR1_fromdB_LOOKUP[ly]; /* be certain */
    return(1);
  }
  memset(out,0,sizeof(*out)*n);
  return(0);
}

/* export hooks */
const vorbis_func_floor floor1_exportbundle={
  &floor1_pack,&floor1_unpack,&floor1_look,&floor1_free_info,
  &floor1_free_look,&floor1_inverse1,&floor1_inverse2
};
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2015             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: floor backend 0 implementation

 ********************************************************************/

#include <stdlib.h>
#include <string.h>
#include <math.h>
/*#include <ogg/ogg.h>*/
/*#include "vorbis/codec.h"*/
/*#include "codec_internal.h"*/
/*#include "registry.h"*/
/*#include "lpc.h"*/
/*#include "lsp.h"*/
/*#include "codebook.h"*/
/*#include "scales.h"*/
/*#include "misc.h"*/
/*#include "os.h"*/

/*#include "misc.h"*/
#include <stdio.h>

typedef struct {
  int ln;
  int  m;
  int **linearmap;
  int  n[2];

  vorbis_info_floor0 *vi;

  long bits;
  long frames;
} vorbis_look_floor0;


/***********************************************/

static void floor0_free_info(vorbis_info_floor *i){
  vorbis_info_floor0 *info=(vorbis_info_floor0 *)i;
  if(info){
    memset(info,0,sizeof(*info));
    _ogg_free(info);
  }
}

static void floor0_free_look(vorbis_look_floor *i){
  vorbis_look_floor0 *look=(vorbis_look_floor0 *)i;
  if(look){

    if(look->linearmap){

      if(look->linearmap[0])_ogg_free(look->linearmap[0]);
      if(look->linearmap[1])_ogg_free(look->linearmap[1]);

      _ogg_free(look->linearmap);
    }
    memset(look,0,sizeof(*look));
    _ogg_free(look);
  }
}

static vorbis_info_floor *floor0_unpack (vorbis_info *vi,oggpack_buffer *opb){
  codec_setup_info     *ci=(codec_setup_info*)vi->codec_setup;
  int j;

  vorbis_info_floor0 *info=(vorbis_info_floor0*)_ogg_malloc(sizeof(*info));
  info->order=oggpack_read(opb,8);
  info->rate=oggpack_read(opb,16);
  info->barkmap=oggpack_read(opb,16);
  info->ampbits=oggpack_read(opb,6);
  info->ampdB=oggpack_read(opb,8);
  info->numbooks=oggpack_read(opb,4)+1;

  if(info->order<1)goto err_out;
  if(info->rate<1)goto err_out;
  if(info->barkmap<1)goto err_out;
  if(info->numbooks<1)goto err_out;

  for(j=0;j<info->numbooks;j++){
    info->books[j]=oggpack_read(opb,8);
    if(info->books[j]<0 || info->books[j]>=ci->books)goto err_out;
    if(ci->book_param[info->books[j]]->maptype==0)goto err_out;
    if(ci->book_param[info->books[j]]->dim<1)goto err_out;
  }
  return(info);

 err_out:
  floor0_free_info(info);
  return(NULL);
}

/* initialize Bark scale and normalization lookups.  We could do this
   with static tables, but Vorbis allows a number of possible
   combinations, so it's best to do it computationally.

   The below is authoritative in terms of defining scale mapping.
   Note that the scale depends on the sampling rate as well as the
   linear block and mapping sizes */

static void floor0_map_lazy_init(vorbis_block      *vb,
                                 vorbis_info_floor *infoX,
                                 vorbis_look_floor0 *look){
  if(!look->linearmap[vb->W]){
    vorbis_dsp_state   *vd=vb->vd;
    vorbis_info        *vi=vd->vi;
    codec_setup_info   *ci=(codec_setup_info*)vi->codec_setup;
    vorbis_info_floor0 *info=(vorbis_info_floor0 *)infoX;
    int W=vb->W;
    int n=ci->blocksizes[W]/2,j;

    /* we choose a scaling constant so that:
       floor(bark(rate/2-1)*C)=mapped-1
     floor(bark(rate/2)*C)=mapped */
    float scale=look->ln/toBARK(info->rate/2.f);

    /* the mapping from a linear scale to a smaller bark scale is
       straightforward.  We do *not* make sure that the linear mapping
       does not skip bark-scale bins; the decoder simply skips them and
       the encoder may do what it wishes in filling them.  They're
       necessary in some mapping combinations to keep the scale spacing
       accurate */
    look->linearmap[W]=(int*)_ogg_malloc((n+1)*sizeof(**look->linearmap));
    for(j=0;j<n;j++){
      int val=floor( toBARK((info->rate/2.f)/n*j)
                     *scale); /* bark numbers represent band edges */
      if(val>=look->ln)val=look->ln-1; /* guard against the approximation */
      look->linearmap[W][j]=val;
    }
    look->linearmap[W][j]=-1;
    look->n[W]=n;
  }
}

static vorbis_look_floor *floor0_look(vorbis_dsp_state *vd,
                                      vorbis_info_floor *i){
  vorbis_info_floor0 *info=(vorbis_info_floor0*)i;
  vorbis_look_floor0 *look=(vorbis_look_floor0*)_ogg_calloc(1,sizeof(*look));

  (void)vd;

  look->m=info->order;
  look->ln=info->barkmap;
  look->vi=info;

  look->linearmap=(int**)_ogg_calloc(2,sizeof(*look->linearmap));

  return look;
}

static void *floor0_inverse1(vorbis_block *vb,vorbis_look_floor *i){
  vorbis_look_floor0 *look=(vorbis_look_floor0 *)i;
  vorbis_info_floor0 *info=look->vi;
  int j,k;

  int ampraw=oggpack_read(&vb->opb,info->ampbits);
  if(ampraw>0){ /* also handles the -1 out of data case */
    long maxval=(1<<info->ampbits)-1;
    float amp=(float)ampraw/maxval*info->ampdB;
    int booknum=oggpack_read(&vb->opb,ov_ilog(info->numbooks));

    if(booknum!=-1 && booknum<info->numbooks){ /* be paranoid */
      codec_setup_info  *ci=(codec_setup_info*)vb->vd->vi->codec_setup;
      codebook *b=ci->fullbooks+info->books[booknum];
      float last=0.f;

      /* the additional b->dim is a guard against any possible stack
         smash; b->dim is provably more than we can overflow the
         vector */
      float *lsp=(float*)_vorbis_block_alloc(vb,sizeof(*lsp)*(look->m+b->dim+1));

      if(vorbis_book_decodev_set(b,lsp,&vb->opb,look->m)==-1)goto eop;
      for(j=0;j<look->m;){
        for(k=0;j<look->m && k<b->dim;k++,j++)lsp[j]+=last;
        last=lsp[j-1];
      }

      lsp[look->m]=amp;
      return(lsp);
    }
  }
 eop:
  return(NULL);
}

static int floor0_inverse2(vorbis_block *vb,vorbis_look_floor *i,
                           void *memo,float *out){
  vorbis_look_floor0 *look=(vorbis_look_floor0 *)i;
  vorbis_info_floor0 *info=look->vi;

  floor0_map_lazy_init(vb,info,look);

  if(memo){
    float *lsp=(float *)memo;
    float amp=lsp[look->m];

    /* take the coefficients back to a spectral envelope curve */
    vorbis_lsp_to_curve(out,
                        look->linearmap[vb->W],
                        look->n[vb->W],
                        look->ln,
                        lsp,look->m,amp,(float)info->ampdB);
    return(1);
  }
  memset(out,0,sizeof(*out)*look->n[vb->W]);
  return(0);
}

/* export hooks */
const vorbis_func_floor floor0_exportbundle={
  NULL,&floor0_unpack,&floor0_look,&floor0_free_info,
  &floor0_free_look,&floor0_inverse1,&floor0_inverse2
};
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2010             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: residue backend 0, 1 and 2 implementation

 ********************************************************************/

/* Slow, slow, slow, simpleminded and did I mention it was slow?  The
   encode/decode loops are coded for clarity and performance is not
   yet even a nagging little idea lurking in the shadows.  Oh and BTW,
   it's slow. */

#include <stdlib.h>
#include <string.h>
#include <math.h>
/*#include <ogg/ogg.h>*/
/*#include "vorbis/codec.h"*/
/*#include "codec_internal.h"*/
/*#include "registry.h"*/
/*#include "codebook.h"*/
/*#include "misc.h"*/
/*#include "os.h"*/

#if defined(TRAIN_RES) || defined (TRAIN_RESAUX)
#include <stdio.h>
#endif

typedef struct {
  vorbis_info_residue0 *info;

  int         parts;
  int         stages;
  codebook   *fullbooks;
  codebook   *phrasebook;
  codebook ***partbooks;

  int         partvals;
  int       **decodemap;

  long      postbits;
  long      phrasebits;
  long      frames;

#if defined(TRAIN_RES) || defined(TRAIN_RESAUX)
  int        train_seq;
  long      *training_data[8][64];
  float      training_max[8][64];
  float      training_min[8][64];
  float     tmin;
  float     tmax;
  int       submap;
#endif

} vorbis_look_residue0;

void res0_free_info(vorbis_info_residue *i){
  vorbis_info_residue0 *info=(vorbis_info_residue0 *)i;
  if(info){
    memset(info,0,sizeof(*info));
    _ogg_free(info);
  }
}

void res0_free_look(vorbis_look_residue *i){
  int j;
  if(i){

    vorbis_look_residue0 *look=(vorbis_look_residue0 *)i;

#ifdef TRAIN_RES
    {
      int j,k,l;
      for(j=0;j<look->parts;j++){
        /*fprintf(stderr,"partition %d: ",j);*/
        for(k=0;k<8;k++)
          if(look->training_data[k][j]){
            char buffer[80];
            FILE *of;
            codebook *statebook=look->partbooks[j][k];

            /* long and short into the same bucket by current convention */
            sprintf(buffer,"res_sub%d_part%d_pass%d.vqd",look->submap,j,k);
            of=fopen(buffer,"a");

            for(l=0;l<statebook->entries;l++)
              fprintf(of,"%d:%ld\n",l,look->training_data[k][j][l]);

            fclose(of);

            /*fprintf(stderr,"%d(%.2f|%.2f) ",k,
              look->training_min[k][j],look->training_max[k][j]);*/

            _ogg_free(look->training_data[k][j]);
            look->training_data[k][j]=NULL;
          }
        /*fprintf(stderr,"\n");*/
      }
    }
    fprintf(stderr,"min/max residue: %g::%g\n",look->tmin,look->tmax);

    /*fprintf(stderr,"residue bit usage %f:%f (%f total)\n",
            (float)look->phrasebits/look->frames,
            (float)look->postbits/look->frames,
            (float)(look->postbits+look->phrasebits)/look->frames);*/
#endif


    /*vorbis_info_residue0 *info=look->info;

    fprintf(stderr,
            "%ld frames encoded in %ld phrasebits and %ld residue bits "
            "(%g/frame) \n",look->frames,look->phrasebits,
            look->resbitsflat,
            (look->phrasebits+look->resbitsflat)/(float)look->frames);

    for(j=0;j<look->parts;j++){
      long acc=0;
      fprintf(stderr,"\t[%d] == ",j);
      for(k=0;k<look->stages;k++)
        if((info->secondstages[j]>>k)&1){
          fprintf(stderr,"%ld,",look->resbits[j][k]);
          acc+=look->resbits[j][k];
        }

      fprintf(stderr,":: (%ld vals) %1.2fbits/sample\n",look->resvals[j],
              acc?(float)acc/(look->resvals[j]*info->grouping):0);
    }
    fprintf(stderr,"\n");*/

    for(j=0;j<look->parts;j++)
      if(look->partbooks[j])_ogg_free(look->partbooks[j]);
    _ogg_free(look->partbooks);
    for(j=0;j<look->partvals;j++)
      _ogg_free(look->decodemap[j]);
    _ogg_free(look->decodemap);

    memset(look,0,sizeof(*look));
    _ogg_free(look);
  }
}

static int icount(unsigned int v){
  int ret=0;
  while(v){
    ret+=v&1;
    v>>=1;
  }
  return(ret);
}


void res0_pack(vorbis_info_residue *vr,oggpack_buffer *opb){
  vorbis_info_residue0 *info=(vorbis_info_residue0 *)vr;
  int j,acc=0;
  oggpack_write(opb,info->begin,24);
  oggpack_write(opb,info->end,24);

  oggpack_write(opb,info->grouping-1,24);  /* residue vectors to group and
                                             code with a partitioned book */
  oggpack_write(opb,info->partitions-1,6); /* possible partition choices */
  oggpack_write(opb,info->groupbook,8);  /* group huffman book */

  /* secondstages is a bitmask; as encoding progresses pass by pass, a
     bitmask of one indicates this partition class has bits to write
     this pass */
  for(j=0;j<info->partitions;j++){
    if(ov_ilog(info->secondstages[j])>3){
      /* yes, this is a minor hack due to not thinking ahead */
      oggpack_write(opb,info->secondstages[j],3);
      oggpack_write(opb,1,1);
      oggpack_write(opb,info->secondstages[j]>>3,5);
    }else
      oggpack_write(opb,info->secondstages[j],4); /* trailing zero */
    acc+=icount(info->secondstages[j]);
  }
  for(j=0;j<acc;j++)
    oggpack_write(opb,info->booklist[j],8);

}

/* vorbis_info is for range checking */
vorbis_info_residue *res0_unpack(vorbis_info *vi,oggpack_buffer *opb){
  int j,acc=0;
  vorbis_info_residue0 *info=(vorbis_info_residue0*)_ogg_calloc(1,sizeof(*info));
  codec_setup_info     *ci=(codec_setup_info*)vi->codec_setup;

  info->begin=oggpack_read(opb,24);
  info->end=oggpack_read(opb,24);
  info->grouping=oggpack_read(opb,24)+1;
  info->partitions=oggpack_read(opb,6)+1;
  info->groupbook=oggpack_read(opb,8);

  /* check for premature EOP */
  if(info->groupbook<0)goto errout;

  for(j=0;j<info->partitions;j++){
    int cascade=oggpack_read(opb,3);
    int cflag=oggpack_read(opb,1);
    if(cflag<0) goto errout;
    if(cflag){
      int c=oggpack_read(opb,5);
      if(c<0) goto errout;
      cascade|=(c<<3);
    }
    info->secondstages[j]=cascade;

    acc+=icount(cascade);
  }
  for(j=0;j<acc;j++){
    int book=oggpack_read(opb,8);
    if(book<0) goto errout;
    info->booklist[j]=book;
  }

  if(info->groupbook>=ci->books)goto errout;
  for(j=0;j<acc;j++){
    if(info->booklist[j]>=ci->books)goto errout;
    if(ci->book_param[info->booklist[j]]->maptype==0)goto errout;
  }

  /* verify the phrasebook is not specifying an impossible or
     inconsistent partitioning scheme. */
  /* modify the phrasebook ranging check from r16327; an early beta
     encoder had a bug where it used an oversized phrasebook by
     accident.  These files should continue to be playable, but don't
     allow an exploit */
  {
    int entries = ci->book_param[info->groupbook]->entries;
    int dim = ci->book_param[info->groupbook]->dim;
    int partvals = 1;
    if (dim<1) goto errout;
    while(dim>0){
      partvals *= info->partitions;
      if(partvals > entries) goto errout;
      dim--;
    }
    info->partvals = partvals;
  }

  return(info);
 errout:
  res0_free_info(info);
  return(NULL);
}

vorbis_look_residue *res0_look(vorbis_dsp_state *vd,
                               vorbis_info_residue *vr){
  vorbis_info_residue0 *info=(vorbis_info_residue0 *)vr;
  vorbis_look_residue0 *look=(vorbis_look_residue0*)_ogg_calloc(1,sizeof(*look));
  codec_setup_info     *ci=(codec_setup_info*)vd->vi->codec_setup;

  int j,k,acc=0;
  int dim;
  int maxstage=0;
  look->info=info;

  look->parts=info->partitions;
  look->fullbooks=ci->fullbooks;
  look->phrasebook=ci->fullbooks+info->groupbook;
  dim=look->phrasebook->dim;

  look->partbooks=(codebook***)_ogg_calloc(look->parts,sizeof(*look->partbooks));

  for(j=0;j<look->parts;j++){
    int stages=ov_ilog(info->secondstages[j]);
    if(stages){
      if(stages>maxstage)maxstage=stages;
      look->partbooks[j]=(codebook**)_ogg_calloc(stages,sizeof(*look->partbooks[j]));
      for(k=0;k<stages;k++)
        if(info->secondstages[j]&(1<<k)){
          look->partbooks[j][k]=ci->fullbooks+info->booklist[acc++];
#ifdef TRAIN_RES
          look->training_data[k][j]=_ogg_calloc(look->partbooks[j][k]->entries,
                                           sizeof(***look->training_data));
#endif
        }
    }
  }

  look->partvals=1;
  for(j=0;j<dim;j++)
      look->partvals*=look->parts;

  look->stages=maxstage;
  look->decodemap=(int**)_ogg_malloc(look->partvals*sizeof(*look->decodemap));
  for(j=0;j<look->partvals;j++){
    long val=j;
    long mult=look->partvals/look->parts;
    look->decodemap[j]=(int*)_ogg_malloc(dim*sizeof(*look->decodemap[j]));
    for(k=0;k<dim;k++){
      long deco=val/mult;
      val-=deco*mult;
      mult/=look->parts;
      look->decodemap[j][k]=deco;
    }
  }
#if defined(TRAIN_RES) || defined (TRAIN_RESAUX)
  {
    static int train_seq=0;
    look->train_seq=train_seq++;
  }
#endif
  return(look);
}

/* break an abstraction and copy some code for performance purposes */
static int local_book_besterror(codebook *book,int *a){
  int dim=book->dim;
  int i,j,o;
  int minval=book->minval;
  int del=book->delta;
  int qv=book->quantvals;
  int ze=(qv>>1);
  int index=0;
  /* assumes integer/centered encoder codebook maptype 1 no more than dim 8 */
  int p[8]={0,0,0,0,0,0,0,0};

  if(del!=1){
    for(i=0,o=dim;i<dim;i++){
      int v = (a[--o]-minval+(del>>1))/del;
      int m = (v<ze ? ((ze-v)<<1)-1 : ((v-ze)<<1));
      index = index*qv+ (m<0?0:(m>=qv?qv-1:m));
      p[o]=v*del+minval;
    }
  }else{
    for(i=0,o=dim;i<dim;i++){
      int v = a[--o]-minval;
      int m = (v<ze ? ((ze-v)<<1)-1 : ((v-ze)<<1));
      index = index*qv+ (m<0?0:(m>=qv?qv-1:m));
      p[o]=v*del+minval;
    }
  }

  if(book->c->lengthlist[index]<=0){
    const static_codebook *c=book->c;
    int best=-1;
    /* assumes integer/centered encoder codebook maptype 1 no more than dim 8 */
    int e[8]={0,0,0,0,0,0,0,0};
    int maxval = book->minval + book->delta*(book->quantvals-1);
    for(i=0;i<book->entries;i++){
      if(c->lengthlist[i]>0){
        int this_=0;
        for(j=0;j<dim;j++){
          int val=(e[j]-a[j]);
          this_ +=val*val;
        }
        if(best==-1 || this_ <best){
          memcpy(p,e,sizeof(p));
          best= this_;
          index=i;
        }
      }
      /* assumes the value patterning created by the tools in vq/ */
      j=0;
      while(e[j]>=maxval)
        e[j++]=0;
      if(e[j]>=0)
        e[j]+=book->delta;
      e[j]= -e[j];
    }
  }

  if(index>-1){
    for(i=0;i<dim;i++)
      *a++ -= p[i];
  }

  return(index);
}

#ifdef TRAIN_RES
static int _encodepart(oggpack_buffer *opb,int *vec, int n,
                       codebook *book,long *acc){
#else
static int _encodepart(oggpack_buffer *opb,int *vec, int n,
                       codebook *book){
#endif
  int i,bits=0;
  int dim=book->dim;
  int step=n/dim;

  for(i=0;i<step;i++){
    int entry=local_book_besterror(book,vec+i*dim);

#ifdef TRAIN_RES
    if(entry>=0)
      acc[entry]++;
#endif

    bits+=vorbis_book_encode(book,entry,opb);

  }

  return(bits);
}

static long **_01class(vorbis_block *vb,vorbis_look_residue *vl,
                       int **in,int ch){
  long i,j,k;
  vorbis_look_residue0 *look=(vorbis_look_residue0 *)vl;
  vorbis_info_residue0 *info=look->info;

  /* move all this setup out later */
  int samples_per_partition=info->grouping;
  int possible_partitions=info->partitions;
  int n=info->end-info->begin;

  int partvals=n/samples_per_partition;
  long **partword=(long**)_vorbis_block_alloc(vb,ch*sizeof(*partword));
  float scale=100./samples_per_partition;

  /* we find the partition type for each partition of each
     channel.  We'll go back and do the interleaved encoding in a
     bit.  For now, clarity */

  for(i=0;i<ch;i++){
    partword[i]=(long*)_vorbis_block_alloc(vb,n/samples_per_partition*sizeof(*partword[i]));
    memset(partword[i],0,n/samples_per_partition*sizeof(*partword[i]));
  }

  for(i=0;i<partvals;i++){
    int offset=i*samples_per_partition+info->begin;
    for(j=0;j<ch;j++){
      int max=0;
      int ent=0;
      for(k=0;k<samples_per_partition;k++){
        if(abs(in[j][offset+k])>max)max=abs(in[j][offset+k]);
        ent+=abs(in[j][offset+k]);
      }
      ent*=scale;

      for(k=0;k<possible_partitions-1;k++)
        if(max<=info->classmetric1[k] &&
           (info->classmetric2[k]<0 || ent<info->classmetric2[k]))
          break;

      partword[j][i]=k;
    }
  }

#ifdef TRAIN_RESAUX
  {
    FILE *of;
    char buffer[80];

    for(i=0;i<ch;i++){
      sprintf(buffer,"resaux_%d.vqd",look->train_seq);
      of=fopen(buffer,"a");
      for(j=0;j<partvals;j++)
        fprintf(of,"%ld, ",partword[i][j]);
      fprintf(of,"\n");
      fclose(of);
    }
  }
#endif
  look->frames++;

  return(partword);
}

/* designed for stereo or other modes where the partition size is an
   integer multiple of the number of channels encoded in the current
   submap */
static long **_2class(vorbis_block *vb,vorbis_look_residue *vl,int **in,
                      int ch){
  long i,j,k,l;
  vorbis_look_residue0 *look=(vorbis_look_residue0 *)vl;
  vorbis_info_residue0 *info=look->info;

  /* move all this setup out later */
  int samples_per_partition=info->grouping;
  int possible_partitions=info->partitions;
  int n=info->end-info->begin;

  int partvals=n/samples_per_partition;
  long **partword=(long**)_vorbis_block_alloc(vb,sizeof(*partword));

#if defined(TRAIN_RES) || defined (TRAIN_RESAUX)
  FILE *of;
  char buffer[80];
#endif

  partword[0]=(long*)_vorbis_block_alloc(vb,partvals*sizeof(*partword[0]));
  memset(partword[0],0,partvals*sizeof(*partword[0]));

  for(i=0,l=info->begin/ch;i<partvals;i++){
    int magmax=0;
    int angmax=0;
    for(j=0;j<samples_per_partition;j+=ch){
      if(abs(in[0][l])>magmax)magmax=abs(in[0][l]);
      for(k=1;k<ch;k++)
        if(abs(in[k][l])>angmax)angmax=abs(in[k][l]);
      l++;
    }

    for(j=0;j<possible_partitions-1;j++)
      if(magmax<=info->classmetric1[j] &&
         angmax<=info->classmetric2[j])
        break;

    partword[0][i]=j;

  }

#ifdef TRAIN_RESAUX
  sprintf(buffer,"resaux_%d.vqd",look->train_seq);
  of=fopen(buffer,"a");
  for(i=0;i<partvals;i++)
    fprintf(of,"%ld, ",partword[0][i]);
  fprintf(of,"\n");
  fclose(of);
#endif

  look->frames++;

  return(partword);
}

static int _01forward(oggpack_buffer *opb,
                      vorbis_look_residue *vl,
                      int **in,int ch,
                      long **partword,
#ifdef TRAIN_RES
                      int (*encode)(oggpack_buffer *,int *,int,
                                    codebook *,long *),
                      int submap
#else
                      int (*encode)(oggpack_buffer *,int *,int,
                                    codebook *)
#endif
){
  long i,j,k,s;
  vorbis_look_residue0 *look=(vorbis_look_residue0 *)vl;
  vorbis_info_residue0 *info=look->info;

#ifdef TRAIN_RES
  look->submap=submap;
#endif

  /* move all this setup out later */
  int samples_per_partition=info->grouping;
  int possible_partitions=info->partitions;
  int partitions_per_word=look->phrasebook->dim;
  int n=info->end-info->begin;

  int partvals=n/samples_per_partition;
  long resbits[128];
  long resvals[128];

#ifdef TRAIN_RES
  for(i=0;i<ch;i++)
    for(j=info->begin;j<info->end;j++){
      if(in[i][j]>look->tmax)look->tmax=in[i][j];
      if(in[i][j]<look->tmin)look->tmin=in[i][j];
    }
#endif

  memset(resbits,0,sizeof(resbits));
  memset(resvals,0,sizeof(resvals));

  /* we code the partition words for each channel, then the residual
     words for a partition per channel until we've written all the
     residual words for that partition word.  Then write the next
     partition channel words... */

  for(s=0;s<look->stages;s++){

    for(i=0;i<partvals;){

      /* first we encode a partition codeword for each channel */
      if(s==0){
        for(j=0;j<ch;j++){
          long val=partword[j][i];
          for(k=1;k<partitions_per_word;k++){
            val*=possible_partitions;
            if(i+k<partvals)
              val+=partword[j][i+k];
          }

          /* training hack */
          if(val<look->phrasebook->entries)
            look->phrasebits+=vorbis_book_encode(look->phrasebook,val,opb);
#if 0 /*def TRAIN_RES*/
          else
            fprintf(stderr,"!");
#endif

        }
      }

      /* now we encode interleaved residual values for the partitions */
      for(k=0;k<partitions_per_word && i<partvals;k++,i++){
        long offset=i*samples_per_partition+info->begin;

        for(j=0;j<ch;j++){
          if(s==0)resvals[partword[j][i]]+=samples_per_partition;
          if(info->secondstages[partword[j][i]]&(1<<s)){
            codebook *statebook=look->partbooks[partword[j][i]][s];
            if(statebook){
              int ret;
#ifdef TRAIN_RES
              long *accumulator=NULL;
              accumulator=look->training_data[s][partword[j][i]];
              {
                int l;
                int *samples=in[j]+offset;
                for(l=0;l<samples_per_partition;l++){
                  if(samples[l]<look->training_min[s][partword[j][i]])
                    look->training_min[s][partword[j][i]]=samples[l];
                  if(samples[l]>look->training_max[s][partword[j][i]])
                    look->training_max[s][partword[j][i]]=samples[l];
                }
              }
              ret=encode(opb,in[j]+offset,samples_per_partition,
                         statebook,accumulator);
#else
              ret=encode(opb,in[j]+offset,samples_per_partition,
                         statebook);
#endif

              look->postbits+=ret;
              resbits[partword[j][i]]+=ret;
            }
          }
        }
      }
    }
  }

  return(0);
}

/* a truncated packet here just means 'stop working'; it's not an error */
static int _01inverse(vorbis_block *vb,vorbis_look_residue *vl,
                      float **in,int ch,
                      long (*decodepart)(codebook *, float *,
                                         oggpack_buffer *,int)){

  long i,j,k,l,s;
  vorbis_look_residue0 *look=(vorbis_look_residue0 *)vl;
  vorbis_info_residue0 *info=look->info;

  /* move all this setup out later */
  int samples_per_partition=info->grouping;
  int partitions_per_word=look->phrasebook->dim;
  int max=vb->pcmend>>1;
  int end=(info->end<max?info->end:max);
  int n=end-info->begin;

  if(n>0){
    int partvals=n/samples_per_partition;
    int partwords=(partvals+partitions_per_word-1)/partitions_per_word;
    int ***partword=(int***)alloca(ch*sizeof(*partword));

    for(j=0;j<ch;j++)
      partword[j]=(int**)_vorbis_block_alloc(vb,partwords*sizeof(*partword[j]));

    for(s=0;s<look->stages;s++){

      /* each loop decodes on partition codeword containing
         partitions_per_word partitions */
      for(i=0,l=0;i<partvals;l++){
        if(s==0){
          /* fetch the partition word for each channel */
          for(j=0;j<ch;j++){
            int temp=vorbis_book_decode(look->phrasebook,&vb->opb);

            if(temp==-1 || temp>=info->partvals)goto eopbreak;
            partword[j][l]=look->decodemap[temp];
            if(partword[j][l]==NULL)goto errout;
          }
        }

        /* now we decode residual values for the partitions */
        for(k=0;k<partitions_per_word && i<partvals;k++,i++)
          for(j=0;j<ch;j++){
            long offset=info->begin+i*samples_per_partition;
            if(info->secondstages[partword[j][l][k]]&(1<<s)){
              codebook *stagebook=look->partbooks[partword[j][l][k]][s];
              if(stagebook){
                if(decodepart(stagebook,in[j]+offset,&vb->opb,
                              samples_per_partition)==-1)goto eopbreak;
              }
            }
          }
      }
    }
  }
 errout:
 eopbreak:
  return(0);
}

int res0_inverse(vorbis_block *vb,vorbis_look_residue *vl,
                 float **in,int *nonzero,int ch){
  int i,used=0;
  for(i=0;i<ch;i++)
    if(nonzero[i])
      in[used++]=in[i];
  if(used)
    return(_01inverse(vb,vl,in,used,vorbis_book_decodevs_add));
  else
    return(0);
}

int res1_forward(oggpack_buffer *opb,vorbis_block *vb,vorbis_look_residue *vl,
                 int **in,int *nonzero,int ch, long **partword, int submap){
  int i,used=0;
  (void)vb;
  for(i=0;i<ch;i++)
    if(nonzero[i])
      in[used++]=in[i];

  if(used){
#ifdef TRAIN_RES
    return _01forward(opb,vl,in,used,partword,_encodepart,submap);
#else
    (void)submap;
    return _01forward(opb,vl,in,used,partword,_encodepart);
#endif
  }else{
    return(0);
  }
}

long **res1_class(vorbis_block *vb,vorbis_look_residue *vl,
                  int **in,int *nonzero,int ch){
  int i,used=0;
  for(i=0;i<ch;i++)
    if(nonzero[i])
      in[used++]=in[i];
  if(used)
    return(_01class(vb,vl,in,used));
  else
    return(0);
}

int res1_inverse(vorbis_block *vb,vorbis_look_residue *vl,
                 float **in,int *nonzero,int ch){
  int i,used=0;
  for(i=0;i<ch;i++)
    if(nonzero[i])
      in[used++]=in[i];
  if(used)
    return(_01inverse(vb,vl,in,used,vorbis_book_decodev_add));
  else
    return(0);
}

long **res2_class(vorbis_block *vb,vorbis_look_residue *vl,
                  int **in,int *nonzero,int ch){
  int i,used=0;
  for(i=0;i<ch;i++)
    if(nonzero[i])used++;
  if(used)
    return(_2class(vb,vl,in,ch));
  else
    return(0);
}

/* res2 is slightly more different; all the channels are interleaved
   into a single vector and encoded. */

int res2_forward(oggpack_buffer *opb,
                 vorbis_block *vb,vorbis_look_residue *vl,
                 int **in,int *nonzero,int ch, long **partword,int submap){
  long i,j,k,n=vb->pcmend/2,used=0;

  /* don't duplicate the code; use a working vector hack for now and
     reshape ourselves into a single channel res1 */
  /* ugly; reallocs for each coupling pass :-( */
  int *work=(int*)_vorbis_block_alloc(vb,ch*n*sizeof(*work));
  for(i=0;i<ch;i++){
    int *pcm=in[i];
    if(nonzero[i])used++;
    for(j=0,k=i;j<n;j++,k+=ch)
      work[k]=pcm[j];
  }

  if(used){
#ifdef TRAIN_RES
    return _01forward(opb,vl,&work,1,partword,_encodepart,submap);
#else
    (void)submap;
    return _01forward(opb,vl,&work,1,partword,_encodepart);
#endif
  }else{
    return(0);
  }
}

/* duplicate code here as speed is somewhat more important */
int res2_inverse(vorbis_block *vb,vorbis_look_residue *vl,
                 float **in,int *nonzero,int ch){
  long i,k,l,s;
  vorbis_look_residue0 *look=(vorbis_look_residue0 *)vl;
  vorbis_info_residue0 *info=look->info;

  /* move all this setup out later */
  int samples_per_partition=info->grouping;
  int partitions_per_word=look->phrasebook->dim;
  int max=(vb->pcmend*ch)>>1;
  int end=(info->end<max?info->end:max);
  int n=end-info->begin;

  if(n>0){
    int partvals=n/samples_per_partition;
    int partwords=(partvals+partitions_per_word-1)/partitions_per_word;
    int **partword=(int**)_vorbis_block_alloc(vb,partwords*sizeof(*partword));

    for(i=0;i<ch;i++)if(nonzero[i])break;
    if(i==ch)return(0); /* no nonzero vectors */

    for(s=0;s<look->stages;s++){
      for(i=0,l=0;i<partvals;l++){

        if(s==0){
          /* fetch the partition word */
          int temp=vorbis_book_decode(look->phrasebook,&vb->opb);
          if(temp==-1 || temp>=info->partvals)goto eopbreak;
          partword[l]=look->decodemap[temp];
          if(partword[l]==NULL)goto errout;
        }

        /* now we decode residual values for the partitions */
        for(k=0;k<partitions_per_word && i<partvals;k++,i++)
          if(info->secondstages[partword[l][k]]&(1<<s)){
            codebook *stagebook=look->partbooks[partword[l][k]][s];

            if(stagebook){
              if(vorbis_book_decodevv_add(stagebook,in,
                                          i*samples_per_partition+info->begin,ch,
                                          &vb->opb,samples_per_partition)==-1)
                goto eopbreak;
            }
          }
      }
    }
  }
 errout:
 eopbreak:
  return(0);
}


const vorbis_func_residue residue0_exportbundle={
  NULL,
  &res0_unpack,
  &res0_look,
  &res0_free_info,
  &res0_free_look,
  NULL,
  NULL,
  &res0_inverse
};

const vorbis_func_residue residue1_exportbundle={
  &res0_pack,
  &res0_unpack,
  &res0_look,
  &res0_free_info,
  &res0_free_look,
  &res1_class,
  &res1_forward,
  &res1_inverse
};

const vorbis_func_residue residue2_exportbundle={
  &res0_pack,
  &res0_unpack,
  &res0_look,
  &res0_free_info,
  &res0_free_look,
  &res2_class,
  &res2_forward,
  &res2_inverse
};
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2010             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: channel mapping 0 implementation

 ********************************************************************/

#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <math.h>
/*#include <ogg/ogg.h>*/
/*#include "vorbis/codec.h"*/
/*#include "codec_internal.h"*/
/*#include "codebook.h"*/
/*#include "window.h"*/
/*#include "registry.h"*/
/*#include "psy.h"*/
/*#include "misc.h"*/

/* simplistic, wasteful way of doing this (unique lookup for each
   mode/submapping); there should be a central repository for
   identical lookups.  That will require minor work, so I'm putting it
   off as low priority.

   Why a lookup for each backend in a given mode?  Because the
   blocksize is set by the mode, and low backend lookups may require
   parameters from other areas of the mode/mapping */

static void mapping0_free_info(vorbis_info_mapping *i){
  vorbis_info_mapping0 *info=(vorbis_info_mapping0 *)i;
  if(info){
    memset(info,0,sizeof(*info));
    _ogg_free(info);
  }
}

static void mapping0_pack(vorbis_info *vi,vorbis_info_mapping *vm,
                          oggpack_buffer *opb){
  int i;
  vorbis_info_mapping0 *info=(vorbis_info_mapping0 *)vm;

  /* another 'we meant to do it this way' hack...  up to beta 4, we
     packed 4 binary zeros here to signify one submapping in use.  We
     now redefine that to mean four bitflags that indicate use of
     deeper features; bit0:submappings, bit1:coupling,
     bit2,3:reserved. This is backward compatable with all actual uses
     of the beta code. */

  if(info->submaps>1){
    oggpack_write(opb,1,1);
    oggpack_write(opb,info->submaps-1,4);
  }else
    oggpack_write(opb,0,1);

  if(info->coupling_steps>0){
    oggpack_write(opb,1,1);
    oggpack_write(opb,info->coupling_steps-1,8);

    for(i=0;i<info->coupling_steps;i++){
      oggpack_write(opb,info->coupling_mag[i],ov_ilog(vi->channels-1));
      oggpack_write(opb,info->coupling_ang[i],ov_ilog(vi->channels-1));
    }
  }else
    oggpack_write(opb,0,1);

  oggpack_write(opb,0,2); /* 2,3:reserved */

  /* we don't write the channel submappings if we only have one... */
  if(info->submaps>1){
    for(i=0;i<vi->channels;i++)
      oggpack_write(opb,info->chmuxlist[i],4);
  }
  for(i=0;i<info->submaps;i++){
    oggpack_write(opb,0,8); /* time submap unused */
    oggpack_write(opb,info->floorsubmap[i],8);
    oggpack_write(opb,info->residuesubmap[i],8);
  }
}

/* also responsible for range checking */
static vorbis_info_mapping *mapping0_unpack(vorbis_info *vi,oggpack_buffer *opb){
  int i,b;
  vorbis_info_mapping0 *info=(vorbis_info_mapping0*)_ogg_calloc(1,sizeof(*info));
  codec_setup_info     *ci=(codec_setup_info*)vi->codec_setup;
  if(vi->channels<=0)goto err_out;

  b=oggpack_read(opb,1);
  if(b<0)goto err_out;
  if(b){
    info->submaps=oggpack_read(opb,4)+1;
    if(info->submaps<=0)goto err_out;
  }else
    info->submaps=1;

  b=oggpack_read(opb,1);
  if(b<0)goto err_out;
  if(b){
    info->coupling_steps=oggpack_read(opb,8)+1;
    if(info->coupling_steps<=0)goto err_out;
    for(i=0;i<info->coupling_steps;i++){
      /* vi->channels > 0 is enforced in the caller */
      int testM=info->coupling_mag[i]=
        oggpack_read(opb,ov_ilog(vi->channels-1));
      int testA=info->coupling_ang[i]=
        oggpack_read(opb,ov_ilog(vi->channels-1));

      if(testM<0 ||
         testA<0 ||
         testM==testA ||
         testM>=vi->channels ||
         testA>=vi->channels) goto err_out;
    }

  }

  if(oggpack_read(opb,2)!=0)goto err_out; /* 2,3:reserved */

  if(info->submaps>1){
    for(i=0;i<vi->channels;i++){
      info->chmuxlist[i]=oggpack_read(opb,4);
      if(info->chmuxlist[i]>=info->submaps || info->chmuxlist[i]<0)goto err_out;
    }
  }
  for(i=0;i<info->submaps;i++){
    oggpack_read(opb,8); /* time submap unused */
    info->floorsubmap[i]=oggpack_read(opb,8);
    if(info->floorsubmap[i]>=ci->floors || info->floorsubmap[i]<0)goto err_out;
    info->residuesubmap[i]=oggpack_read(opb,8);
    if(info->residuesubmap[i]>=ci->residues || info->residuesubmap[i]<0)goto err_out;
  }

  return info;

 err_out:
  mapping0_free_info(info);
  return(NULL);
}

/*#include "os.h"*/
/*#include "lpc.h"*/
/*#include "lsp.h"*/
/*#include "envelope.h"*/
/*#include "mdct.h"*/
/*#include "psy.h"*/
/*#include "scales.h"*/

#if 0
static long seq=0;
static ogg_int64_t total=0;
static float FLOOR1_fromdB_LOOKUP[256]={
  1.0649863e-07F, 1.1341951e-07F, 1.2079015e-07F, 1.2863978e-07F,
  1.3699951e-07F, 1.4590251e-07F, 1.5538408e-07F, 1.6548181e-07F,
  1.7623575e-07F, 1.8768855e-07F, 1.9988561e-07F, 2.128753e-07F,
  2.2670913e-07F, 2.4144197e-07F, 2.5713223e-07F, 2.7384213e-07F,
  2.9163793e-07F, 3.1059021e-07F, 3.3077411e-07F, 3.5226968e-07F,
  3.7516214e-07F, 3.9954229e-07F, 4.2550680e-07F, 4.5315863e-07F,
  4.8260743e-07F, 5.1396998e-07F, 5.4737065e-07F, 5.8294187e-07F,
  6.2082472e-07F, 6.6116941e-07F, 7.0413592e-07F, 7.4989464e-07F,
  7.9862701e-07F, 8.5052630e-07F, 9.0579828e-07F, 9.6466216e-07F,
  1.0273513e-06F, 1.0941144e-06F, 1.1652161e-06F, 1.2409384e-06F,
  1.3215816e-06F, 1.4074654e-06F, 1.4989305e-06F, 1.5963394e-06F,
  1.7000785e-06F, 1.8105592e-06F, 1.9282195e-06F, 2.0535261e-06F,
  2.1869758e-06F, 2.3290978e-06F, 2.4804557e-06F, 2.6416497e-06F,
  2.8133190e-06F, 2.9961443e-06F, 3.1908506e-06F, 3.3982101e-06F,
  3.6190449e-06F, 3.8542308e-06F, 4.1047004e-06F, 4.3714470e-06F,
  4.6555282e-06F, 4.9580707e-06F, 5.2802740e-06F, 5.6234160e-06F,
  5.9888572e-06F, 6.3780469e-06F, 6.7925283e-06F, 7.2339451e-06F,
  7.7040476e-06F, 8.2047000e-06F, 8.7378876e-06F, 9.3057248e-06F,
  9.9104632e-06F, 1.0554501e-05F, 1.1240392e-05F, 1.1970856e-05F,
  1.2748789e-05F, 1.3577278e-05F, 1.4459606e-05F, 1.5399272e-05F,
  1.6400004e-05F, 1.7465768e-05F, 1.8600792e-05F, 1.9809576e-05F,
  2.1096914e-05F, 2.2467911e-05F, 2.3928002e-05F, 2.5482978e-05F,
  2.7139006e-05F, 2.8902651e-05F, 3.0780908e-05F, 3.2781225e-05F,
  3.4911534e-05F, 3.7180282e-05F, 3.9596466e-05F, 4.2169667e-05F,
  4.4910090e-05F, 4.7828601e-05F, 5.0936773e-05F, 5.4246931e-05F,
  5.7772202e-05F, 6.1526565e-05F, 6.5524908e-05F, 6.9783085e-05F,
  7.4317983e-05F, 7.9147585e-05F, 8.4291040e-05F, 8.9768747e-05F,
  9.5602426e-05F, 0.00010181521F, 0.00010843174F, 0.00011547824F,
  0.00012298267F, 0.00013097477F, 0.00013948625F, 0.00014855085F,
  0.00015820453F, 0.00016848555F, 0.00017943469F, 0.00019109536F,
  0.00020351382F, 0.00021673929F, 0.00023082423F, 0.00024582449F,
  0.00026179955F, 0.00027881276F, 0.00029693158F, 0.00031622787F,
  0.00033677814F, 0.00035866388F, 0.00038197188F, 0.00040679456F,
  0.00043323036F, 0.00046138411F, 0.00049136745F, 0.00052329927F,
  0.00055730621F, 0.00059352311F, 0.00063209358F, 0.00067317058F,
  0.00071691700F, 0.00076350630F, 0.00081312324F, 0.00086596457F,
  0.00092223983F, 0.00098217216F, 0.0010459992F, 0.0011139742F,
  0.0011863665F, 0.0012634633F, 0.0013455702F, 0.0014330129F,
  0.0015261382F, 0.0016253153F, 0.0017309374F, 0.0018434235F,
  0.0019632195F, 0.0020908006F, 0.0022266726F, 0.0023713743F,
  0.0025254795F, 0.0026895994F, 0.0028643847F, 0.0030505286F,
  0.0032487691F, 0.0034598925F, 0.0036847358F, 0.0039241906F,
  0.0041792066F, 0.0044507950F, 0.0047400328F, 0.0050480668F,
  0.0053761186F, 0.0057254891F, 0.0060975636F, 0.0064938176F,
  0.0069158225F, 0.0073652516F, 0.0078438871F, 0.0083536271F,
  0.0088964928F, 0.009474637F, 0.010090352F, 0.010746080F,
  0.011444421F, 0.012188144F, 0.012980198F, 0.013823725F,
  0.014722068F, 0.015678791F, 0.016697687F, 0.017782797F,
  0.018938423F, 0.020169149F, 0.021479854F, 0.022875735F,
  0.024362330F, 0.025945531F, 0.027631618F, 0.029427276F,
  0.031339626F, 0.033376252F, 0.035545228F, 0.037855157F,
  0.040315199F, 0.042935108F, 0.045725273F, 0.048696758F,
  0.051861348F, 0.055231591F, 0.058820850F, 0.062643361F,
  0.066714279F, 0.071049749F, 0.075666962F, 0.080584227F,
  0.085821044F, 0.091398179F, 0.097337747F, 0.10366330F,
  0.11039993F, 0.11757434F, 0.12521498F, 0.13335215F,
  0.14201813F, 0.15124727F, 0.16107617F, 0.17154380F,
  0.18269168F, 0.19456402F, 0.20720788F, 0.22067342F,
  0.23501402F, 0.25028656F, 0.26655159F, 0.28387361F,
  0.30232132F, 0.32196786F, 0.34289114F, 0.36517414F,
  0.38890521F, 0.41417847F, 0.44109412F, 0.46975890F,
  0.50028648F, 0.53279791F, 0.56742212F, 0.60429640F,
  0.64356699F, 0.68538959F, 0.72993007F, 0.77736504F,
  0.82788260F, 0.88168307F, 0.9389798F, 1.F,
};

#endif


static int mapping0_forward(vorbis_block *vb){
  vorbis_dsp_state      *vd=vb->vd;
  vorbis_info           *vi=vd->vi;
  codec_setup_info      *ci=(codec_setup_info*)vi->codec_setup;
  private_state         *b=(private_state*)vb->vd->backend_state;
  vorbis_block_internal *vbi=(vorbis_block_internal *)vb->internal;
  int                    n=vb->pcmend;
  int i,j,k;

  int    *nonzero    = (int*)alloca(sizeof(*nonzero)*vi->channels);
  float  **gmdct     = (float**)_vorbis_block_alloc(vb,vi->channels*sizeof(*gmdct));
  int    **iwork      = (int**)_vorbis_block_alloc(vb,vi->channels*sizeof(*iwork));
  int ***floor_posts = (int***)_vorbis_block_alloc(vb,vi->channels*sizeof(*floor_posts));

  float global_ampmax=vbi->ampmax;
  float *local_ampmax=(float*)alloca(sizeof(*local_ampmax)*vi->channels);
  int blocktype=vbi->blocktype;

  int modenumber=vb->W;
  vorbis_info_mapping0 *info=(vorbis_info_mapping0*)ci->map_param[modenumber];
  vorbis_look_psy *psy_look=b->psy+blocktype+(vb->W?2:0);

  vb->mode=modenumber;

  for(i=0;i<vi->channels;i++){
    float scale=4.f/n;
    float scale_dB;

    float *pcm     =vb->pcm[i];
    float *logfft  =pcm;

    iwork[i]=(int*)_vorbis_block_alloc(vb,n/2*sizeof(**iwork));
    gmdct[i]=(float*)_vorbis_block_alloc(vb,n/2*sizeof(**gmdct));

    scale_dB=todB(&scale) + .345; /* + .345 is a hack; the original
                                     todB estimation used on IEEE 754
                                     compliant machines had a bug that
                                     returned dB values about a third
                                     of a decibel too high.  The bug
                                     was harmless because tunings
                                     implicitly took that into
                                     account.  However, fixing the bug
                                     in the estimator requires
                                     changing all the tunings as well.
                                     For now, it's easier to sync
                                     things back up here, and
                                     recalibrate the tunings in the
                                     next major model upgrade. */

#if 0
    if(vi->channels==2){
      if(i==0)
        _analysis_output("pcmL",seq,pcm,n,0,0,total-n/2);
      else
        _analysis_output("pcmR",seq,pcm,n,0,0,total-n/2);
    }else{
      _analysis_output("pcm",seq,pcm,n,0,0,total-n/2);
    }
#endif

    /* window the PCM data */
    _vorbis_apply_window(pcm,b->window,ci->blocksizes,vb->lW,vb->W,vb->nW);

#if 0
    if(vi->channels==2){
      if(i==0)
        _analysis_output("windowedL",seq,pcm,n,0,0,total-n/2);
      else
        _analysis_output("windowedR",seq,pcm,n,0,0,total-n/2);
    }else{
      _analysis_output("windowed",seq,pcm,n,0,0,total-n/2);
    }
#endif

    /* transform the PCM data */
    /* only MDCT right now.... */
    mdct_forward((mdct_lookup*)b->transform[vb->W][0],pcm,gmdct[i]);

    /* FFT yields more accurate tonal estimation (not phase sensitive) */
    drft_forward(&b->fft_look[vb->W],pcm);
    logfft[0]=scale_dB+todB(pcm)  + .345; /* + .345 is a hack; the
                                     original todB estimation used on
                                     IEEE 754 compliant machines had a
                                     bug that returned dB values about
                                     a third of a decibel too high.
                                     The bug was harmless because
                                     tunings implicitly took that into
                                     account.  However, fixing the bug
                                     in the estimator requires
                                     changing all the tunings as well.
                                     For now, it's easier to sync
                                     things back up here, and
                                     recalibrate the tunings in the
                                     next major model upgrade. */
    local_ampmax[i]=logfft[0];
    for(j=1;j<n-1;j+=2){
      float temp=pcm[j]*pcm[j]+pcm[j+1]*pcm[j+1];
      temp=logfft[(j+1)>>1]=scale_dB+.5f*todB(&temp)  + .345; /* +
                                     .345 is a hack; the original todB
                                     estimation used on IEEE 754
                                     compliant machines had a bug that
                                     returned dB values about a third
                                     of a decibel too high.  The bug
                                     was harmless because tunings
                                     implicitly took that into
                                     account.  However, fixing the bug
                                     in the estimator requires
                                     changing all the tunings as well.
                                     For now, it's easier to sync
                                     things back up here, and
                                     recalibrate the tunings in the
                                     next major model upgrade. */
      if(temp>local_ampmax[i])local_ampmax[i]=temp;
    }

    if(local_ampmax[i]>0.f)local_ampmax[i]=0.f;
    if(local_ampmax[i]>global_ampmax)global_ampmax=local_ampmax[i];

#if 0
    if(vi->channels==2){
      if(i==0){
        _analysis_output("fftL",seq,logfft,n/2,1,0,0);
      }else{
        _analysis_output("fftR",seq,logfft,n/2,1,0,0);
      }
    }else{
      _analysis_output("fft",seq,logfft,n/2,1,0,0);
    }
#endif

  }

  {
    float   *noise        = (float*)_vorbis_block_alloc(vb,n/2*sizeof(*noise));
    float   *tone         = (float*)_vorbis_block_alloc(vb,n/2*sizeof(*tone));

    for(i=0;i<vi->channels;i++){
      /* the encoder setup assumes that all the modes used by any
         specific bitrate tweaking use the same floor */

      int submap=info->chmuxlist[i];

      /* the following makes things clearer to *me* anyway */
      float *mdct    =gmdct[i];
      float *logfft  =vb->pcm[i];

      float *logmdct =logfft+n/2;
      float *logmask =logfft;

      vb->mode=modenumber;

      floor_posts[i]=(int**)_vorbis_block_alloc(vb,PACKETBLOBS*sizeof(**floor_posts));
      memset(floor_posts[i],0,sizeof(**floor_posts)*PACKETBLOBS);

      for(j=0;j<n/2;j++)
        logmdct[j]=todB(mdct+j)  + .345; /* + .345 is a hack; the original
                                     todB estimation used on IEEE 754
                                     compliant machines had a bug that
                                     returned dB values about a third
                                     of a decibel too high.  The bug
                                     was harmless because tunings
                                     implicitly took that into
                                     account.  However, fixing the bug
                                     in the estimator requires
                                     changing all the tunings as well.
                                     For now, it's easier to sync
                                     things back up here, and
                                     recalibrate the tunings in the
                                     next major model upgrade. */

#if 0
      if(vi->channels==2){
        if(i==0)
          _analysis_output("mdctL",seq,logmdct,n/2,1,0,0);
        else
          _analysis_output("mdctR",seq,logmdct,n/2,1,0,0);
      }else{
        _analysis_output("mdct",seq,logmdct,n/2,1,0,0);
      }
#endif

      /* first step; noise masking.  Not only does 'noise masking'
         give us curves from which we can decide how much resolution
         to give noise parts of the spectrum, it also implicitly hands
         us a tonality estimate (the larger the value in the
         'noise_depth' vector, the more tonal that area is) */

      _vp_noisemask(psy_look,
                    logmdct,
                    noise); /* noise does not have by-frequency offset
                               bias applied yet */
#if 0
      if(vi->channels==2){
        if(i==0)
          _analysis_output("noiseL",seq,noise,n/2,1,0,0);
        else
          _analysis_output("noiseR",seq,noise,n/2,1,0,0);
      }else{
        _analysis_output("noise",seq,noise,n/2,1,0,0);
      }
#endif

      /* second step: 'all the other crap'; all the stuff that isn't
         computed/fit for bitrate management goes in the second psy
         vector.  This includes tone masking, peak limiting and ATH */

      _vp_tonemask(psy_look,
                   logfft,
                   tone,
                   global_ampmax,
                   local_ampmax[i]);

#if 0
      if(vi->channels==2){
        if(i==0)
          _analysis_output("toneL",seq,tone,n/2,1,0,0);
        else
          _analysis_output("toneR",seq,tone,n/2,1,0,0);
      }else{
        _analysis_output("tone",seq,tone,n/2,1,0,0);
      }
#endif

      /* third step; we offset the noise vectors, overlay tone
         masking.  We then do a floor1-specific line fit.  If we're
         performing bitrate management, the line fit is performed
         multiple times for up/down tweakage on demand. */

#if 0
      {
      float aotuv[psy_look->n];
#endif

        _vp_offset_and_mix(psy_look,
                           noise,
                           tone,
                           1,
                           logmask,
                           mdct,
                           logmdct);

#if 0
        if(vi->channels==2){
          if(i==0)
            _analysis_output("aotuvM1_L",seq,aotuv,psy_look->n,1,1,0);
          else
            _analysis_output("aotuvM1_R",seq,aotuv,psy_look->n,1,1,0);
        }else{
          _analysis_output("aotuvM1",seq,aotuv,psy_look->n,1,1,0);
        }
      }
#endif


#if 0
      if(vi->channels==2){
        if(i==0)
          _analysis_output("mask1L",seq,logmask,n/2,1,0,0);
        else
          _analysis_output("mask1R",seq,logmask,n/2,1,0,0);
      }else{
        _analysis_output("mask1",seq,logmask,n/2,1,0,0);
      }
#endif

      /* this algorithm is hardwired to floor 1 for now; abort out if
         we're *not* floor1.  This won't happen unless someone has
         broken the encode setup lib.  Guard it anyway. */
      if(ci->floor_type[info->floorsubmap[submap]]!=1)return(-1);

      floor_posts[i][PACKETBLOBS/2]=
        floor1_fit(vb,(vorbis_look_floor1*)b->flr[info->floorsubmap[submap]],
                   logmdct,
                   logmask);

      /* are we managing bitrate?  If so, perform two more fits for
         later rate tweaking (fits represent hi/lo) */
      if(vorbis_bitrate_managed(vb) && floor_posts[i][PACKETBLOBS/2]){
        /* higher rate by way of lower noise curve */

        _vp_offset_and_mix(psy_look,
                           noise,
                           tone,
                           2,
                           logmask,
                           mdct,
                           logmdct);

#if 0
        if(vi->channels==2){
          if(i==0)
            _analysis_output("mask2L",seq,logmask,n/2,1,0,0);
          else
            _analysis_output("mask2R",seq,logmask,n/2,1,0,0);
        }else{
          _analysis_output("mask2",seq,logmask,n/2,1,0,0);
        }
#endif

        floor_posts[i][PACKETBLOBS-1]=
          floor1_fit(vb,(vorbis_look_floor1*)b->flr[info->floorsubmap[submap]],
                     logmdct,
                     logmask);

        /* lower rate by way of higher noise curve */
        _vp_offset_and_mix(psy_look,
                           noise,
                           tone,
                           0,
                           logmask,
                           mdct,
                           logmdct);

#if 0
        if(vi->channels==2){
          if(i==0)
            _analysis_output("mask0L",seq,logmask,n/2,1,0,0);
          else
            _analysis_output("mask0R",seq,logmask,n/2,1,0,0);
        }else{
          _analysis_output("mask0",seq,logmask,n/2,1,0,0);
        }
#endif

        floor_posts[i][0]=
          floor1_fit(vb,(vorbis_look_floor1*)b->flr[info->floorsubmap[submap]],
                     logmdct,
                     logmask);

        /* we also interpolate a range of intermediate curves for
           intermediate rates */
        for(k=1;k<PACKETBLOBS/2;k++)
          floor_posts[i][k]=
            floor1_interpolate_fit(vb,(vorbis_look_floor1*)b->flr[info->floorsubmap[submap]],
                                   floor_posts[i][0],
                                   floor_posts[i][PACKETBLOBS/2],
                                   k*65536/(PACKETBLOBS/2));
        for(k=PACKETBLOBS/2+1;k<PACKETBLOBS-1;k++)
          floor_posts[i][k]=
            floor1_interpolate_fit(vb,(vorbis_look_floor1*)b->flr[info->floorsubmap[submap]],
                                   floor_posts[i][PACKETBLOBS/2],
                                   floor_posts[i][PACKETBLOBS-1],
                                   (k-PACKETBLOBS/2)*65536/(PACKETBLOBS/2));
      }
    }
  }
  vbi->ampmax=global_ampmax;

  /*
    the next phases are performed once for vbr-only and PACKETBLOB
    times for bitrate managed modes.

    1) encode actual mode being used
    2) encode the floor for each channel, compute coded mask curve/res
    3) normalize and couple.
    4) encode residue
    5) save packet bytes to the packetblob vector

  */

  /* iterate over the many masking curve fits we've created */

  {
    int **couple_bundle=(int**)alloca(sizeof(*couple_bundle)*vi->channels);
    int *zerobundle=(int*)alloca(sizeof(*zerobundle)*vi->channels);

    for(k=(vorbis_bitrate_managed(vb)?0:PACKETBLOBS/2);
        k<=(vorbis_bitrate_managed(vb)?PACKETBLOBS-1:PACKETBLOBS/2);
        k++){
      oggpack_buffer *opb=vbi->packetblob[k];

      /* start out our new packet blob with packet type and mode */
      /* Encode the packet type */
      oggpack_write(opb,0,1);
      /* Encode the modenumber */
      /* Encode frame mode, pre,post windowsize, then dispatch */
      oggpack_write(opb,modenumber,b->modebits);
      if(vb->W){
        oggpack_write(opb,vb->lW,1);
        oggpack_write(opb,vb->nW,1);
      }

      /* encode floor, compute masking curve, sep out residue */
      for(i=0;i<vi->channels;i++){
        int submap=info->chmuxlist[i];
        int *ilogmask=iwork[i];

        nonzero[i]=floor1_encode(opb,vb,(vorbis_look_floor1*)b->flr[info->floorsubmap[submap]],
                                 floor_posts[i][k],
                                 ilogmask);
#if 0
        {
          char buf[80];
          sprintf(buf,"maskI%c%d",i?'R':'L',k);
          float work[n/2];
          for(j=0;j<n/2;j++)
            work[j]=FLOOR1_fromdB_LOOKUP[iwork[i][j]];
          _analysis_output(buf,seq,work,n/2,1,1,0);
        }
#endif
      }

      /* our iteration is now based on masking curve, not prequant and
         coupling.  Only one prequant/coupling step */

      /* quantize/couple */
      /* incomplete implementation that assumes the tree is all depth
         one, or no tree at all */
      _vp_couple_quantize_normalize(k,
                                    &ci->psy_g_param,
                                    psy_look,
                                    info,
                                    gmdct,
                                    iwork,
                                    nonzero,
                                    ci->psy_g_param.sliding_lowpass[vb->W][k],
                                    vi->channels);

#if 0
      for(i=0;i<vi->channels;i++){
        char buf[80];
        sprintf(buf,"res%c%d",i?'R':'L',k);
        float work[n/2];
        for(j=0;j<n/2;j++)
          work[j]=iwork[i][j];
        _analysis_output(buf,seq,work,n/2,1,0,0);
      }
#endif

      /* classify and encode by submap */
      for(i=0;i<info->submaps;i++){
        int ch_in_bundle=0;
        long **classifications;
        int resnum=info->residuesubmap[i];

        for(j=0;j<vi->channels;j++){
          if(info->chmuxlist[j]==i){
            zerobundle[ch_in_bundle]=0;
            if(nonzero[j])zerobundle[ch_in_bundle]=1;
            couple_bundle[ch_in_bundle++]=iwork[j];
          }
        }

        classifications=_residue_P[ci->residue_type[resnum]]->
          class_(vb,b->residue[resnum],couple_bundle,zerobundle,ch_in_bundle);

        ch_in_bundle=0;
        for(j=0;j<vi->channels;j++)
          if(info->chmuxlist[j]==i)
            couple_bundle[ch_in_bundle++]=iwork[j];

        _residue_P[ci->residue_type[resnum]]->
          forward(opb,vb,b->residue[resnum],
                  couple_bundle,zerobundle,ch_in_bundle,classifications,i);
      }

      /* ok, done encoding.  Next protopacket. */
    }

  }

#if 0
  seq++;
  total+=ci->blocksizes[vb->W]/4+ci->blocksizes[vb->nW]/4;
#endif
  return(0);
}

static int mapping0_inverse(vorbis_block *vb,vorbis_info_mapping *l){
  vorbis_dsp_state     *vd=vb->vd;
  vorbis_info          *vi=vd->vi;
  codec_setup_info     *ci=(codec_setup_info*)vi->codec_setup;
  private_state        *b=(private_state*)vd->backend_state;
  vorbis_info_mapping0 *info=(vorbis_info_mapping0 *)l;

  int                   i,j;
  long                  n=vb->pcmend=ci->blocksizes[vb->W];

  float **pcmbundle=(float**)alloca(sizeof(*pcmbundle)*vi->channels);
  int    *zerobundle=(int*)alloca(sizeof(*zerobundle)*vi->channels);

  int   *nonzero  =(int*)alloca(sizeof(*nonzero)*vi->channels);
  void **floormemo=(void**)alloca(sizeof(*floormemo)*vi->channels);

  /* recover the spectral envelope; store it in the PCM vector for now */
  for(i=0;i<vi->channels;i++){
    int submap=info->chmuxlist[i];
    floormemo[i]=_floor_P[ci->floor_type[info->floorsubmap[submap]]]->
      inverse1(vb,b->flr[info->floorsubmap[submap]]);
    if(floormemo[i])
      nonzero[i]=1;
    else
      nonzero[i]=0;
    memset(vb->pcm[i],0,sizeof(*vb->pcm[i])*n/2);
  }

  /* channel coupling can 'dirty' the nonzero listing */
  for(i=0;i<info->coupling_steps;i++){
    if(nonzero[info->coupling_mag[i]] ||
       nonzero[info->coupling_ang[i]]){
      nonzero[info->coupling_mag[i]]=1;
      nonzero[info->coupling_ang[i]]=1;
    }
  }

  /* recover the residue into our working vectors */
  for(i=0;i<info->submaps;i++){
    int ch_in_bundle=0;
    for(j=0;j<vi->channels;j++){
      if(info->chmuxlist[j]==i){
        if(nonzero[j])
          zerobundle[ch_in_bundle]=1;
        else
          zerobundle[ch_in_bundle]=0;
        pcmbundle[ch_in_bundle++]=vb->pcm[j];
      }
    }

    _residue_P[ci->residue_type[info->residuesubmap[i]]]->
      inverse(vb,b->residue[info->residuesubmap[i]],
              pcmbundle,zerobundle,ch_in_bundle);
  }

  /* channel coupling */
  for(i=info->coupling_steps-1;i>=0;i--){
    float *pcmM=vb->pcm[info->coupling_mag[i]];
    float *pcmA=vb->pcm[info->coupling_ang[i]];

    for(j=0;j<n/2;j++){
      float mag=pcmM[j];
      float ang=pcmA[j];

      if(mag>0)
        if(ang>0){
          pcmM[j]=mag;
          pcmA[j]=mag-ang;
        }else{
          pcmA[j]=mag;
          pcmM[j]=mag+ang;
        }
      else
        if(ang>0){
          pcmM[j]=mag;
          pcmA[j]=mag+ang;
        }else{
          pcmA[j]=mag;
          pcmM[j]=mag-ang;
        }
    }
  }

  /* compute and apply spectral envelope */
  for(i=0;i<vi->channels;i++){
    float *pcm=vb->pcm[i];
    int submap=info->chmuxlist[i];
    _floor_P[ci->floor_type[info->floorsubmap[submap]]]->
      inverse2(vb,b->flr[info->floorsubmap[submap]],
               floormemo[i],pcm);
  }

  /* transform the PCM data; takes PCM vector, vb; modifies PCM vector */
  /* only MDCT right now.... */
  for(i=0;i<vi->channels;i++){
    float *pcm=vb->pcm[i];
    mdct_backward((mdct_lookup*)b->transform[vb->W][0],pcm,pcm);
  }

  /* all done! */
  return(0);
}

/* export hooks */
const vorbis_func_mapping mapping0_exportbundle={
  &mapping0_pack,
  &mapping0_unpack,
  &mapping0_free_info,
  &mapping0_forward,
  &mapping0_inverse
};
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: registry for time, floor, res backends and channel mappings

 ********************************************************************/

/*#include "vorbis/codec.h"*/
/*#include "codec_internal.h"*/
/*#include "registry.h"*/
/*#include "misc.h"*/
/* seems like major overkill now; the backend numbers will grow into
   the infrastructure soon enough */

extern const vorbis_func_floor     floor0_exportbundle;
extern const vorbis_func_floor     floor1_exportbundle;
extern const vorbis_func_residue   residue0_exportbundle;
extern const vorbis_func_residue   residue1_exportbundle;
extern const vorbis_func_residue   residue2_exportbundle;
extern const vorbis_func_mapping   mapping0_exportbundle;

const vorbis_func_floor     *const _floor_P[]={
  &floor0_exportbundle,
  &floor1_exportbundle,
};

const vorbis_func_residue   *const _residue_P[]={
  &residue0_exportbundle,
  &residue1_exportbundle,
  &residue2_exportbundle,
};

const vorbis_func_mapping   *const _mapping_P[]={
  &mapping0_exportbundle,
};
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2015             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: basic codebook pack/unpack/code/decode operations

 ********************************************************************/

#include <stdlib.h>
#include <string.h>
#include <math.h>
/*#include <ogg/ogg.h>*/
/*#include "vorbis/codec.h"*/
/*#include "codebook.h"*/
/*#include "scales.h"*/
/*#include "misc.h"*/
/*#include "os.h"*/

/* packs the given codebook into the bitstream **************************/

int vorbis_staticbook_pack(const static_codebook *c,oggpack_buffer *opb){
  long i,j;
  int ordered=0;

  /* first the basic parameters */
  oggpack_write(opb,0x564342,24);
  oggpack_write(opb,c->dim,16);
  oggpack_write(opb,c->entries,24);

  /* pack the codewords.  There are two packings; length ordered and
     length random.  Decide between the two now. */

  for(i=1;i<c->entries;i++)
    if(c->lengthlist[i-1]==0 || c->lengthlist[i]<c->lengthlist[i-1])break;
  if(i==c->entries)ordered=1;

  if(ordered){
    /* length ordered.  We only need to say how many codewords of
       each length.  The actual codewords are generated
       deterministically */

    long count=0;
    oggpack_write(opb,1,1);  /* ordered */
    oggpack_write(opb,c->lengthlist[0]-1,5); /* 1 to 32 */

    for(i=1;i<c->entries;i++){
      char this_=c->lengthlist[i];
      char last=c->lengthlist[i-1];
      if(this_>last){
        for(j=last;j<this_;j++){
          oggpack_write(opb,i-count,ov_ilog(c->entries-count));
          count=i;
        }
      }
    }
    oggpack_write(opb,i-count,ov_ilog(c->entries-count));

  }else{
    /* length random.  Again, we don't code the codeword itself, just
       the length.  This time, though, we have to encode each length */
    oggpack_write(opb,0,1);   /* unordered */

    /* algortihmic mapping has use for 'unused entries', which we tag
       here.  The algorithmic mapping happens as usual, but the unused
       entry has no codeword. */
    for(i=0;i<c->entries;i++)
      if(c->lengthlist[i]==0)break;

    if(i==c->entries){
      oggpack_write(opb,0,1); /* no unused entries */
      for(i=0;i<c->entries;i++)
        oggpack_write(opb,c->lengthlist[i]-1,5);
    }else{
      oggpack_write(opb,1,1); /* we have unused entries; thus we tag */
      for(i=0;i<c->entries;i++){
        if(c->lengthlist[i]==0){
          oggpack_write(opb,0,1);
        }else{
          oggpack_write(opb,1,1);
          oggpack_write(opb,c->lengthlist[i]-1,5);
        }
      }
    }
  }

  /* is the entry number the desired return value, or do we have a
     mapping? If we have a mapping, what type? */
  oggpack_write(opb,c->maptype,4);
  switch(c->maptype){
  case 0:
    /* no mapping */
    break;
  case 1:case 2:
    /* implicitly populated value mapping */
    /* explicitly populated value mapping */

    if(!c->quantlist){
      /* no quantlist?  error */
      return(-1);
    }

    /* values that define the dequantization */
    oggpack_write(opb,c->q_min,32);
    oggpack_write(opb,c->q_delta,32);
    oggpack_write(opb,c->q_quant-1,4);
    oggpack_write(opb,c->q_sequencep,1);

    {
      int quantvals;
      switch(c->maptype){
      case 1:
        /* a single column of (c->entries/c->dim) quantized values for
           building a full value list algorithmically (square lattice) */
        quantvals=_book_maptype1_quantvals(c);
        break;
      case 2:
        /* every value (c->entries*c->dim total) specified explicitly */
        quantvals=c->entries*c->dim;
        break;
      default: /* NOT_REACHABLE */
        quantvals=-1;
      }

      /* quantized values */
      for(i=0;i<quantvals;i++)
        oggpack_write(opb,labs(c->quantlist[i]),c->q_quant);

    }
    break;
  default:
    /* error case; we don't have any other map types now */
    return(-1);
  }

  return(0);
}

/* unpacks a codebook from the packet buffer into the codebook struct,
   readies the codebook auxiliary structures for decode *************/
static_codebook *vorbis_staticbook_unpack(oggpack_buffer *opb){
  long i,j;
  static_codebook *s=(static_codebook*)_ogg_calloc(1,sizeof(*s));
  s->allocedp=1;

  /* make sure alignment is correct */
  if(oggpack_read(opb,24)!=0x564342)goto _eofout;

  /* first the basic parameters */
  s->dim=oggpack_read(opb,16);
  s->entries=oggpack_read(opb,24);
  if(s->entries==-1)goto _eofout;

  if(ov_ilog(s->dim)+ov_ilog(s->entries)>24)goto _eofout;

  /* codeword ordering.... length ordered or unordered? */
  switch((int)oggpack_read(opb,1)){
  case 0:{
    long unused;
    /* allocated but unused entries? */
    unused=oggpack_read(opb,1);
    if((s->entries*(unused?1:5)+7)>>3>opb->storage-oggpack_bytes(opb))
      goto _eofout;
    /* unordered */
    s->lengthlist=(char*)_ogg_malloc(sizeof(*s->lengthlist)*s->entries);

    /* allocated but unused entries? */
    if(unused){
      /* yes, unused entries */

      for(i=0;i<s->entries;i++){
        if(oggpack_read(opb,1)){
          long num=oggpack_read(opb,5);
          if(num==-1)goto _eofout;
          s->lengthlist[i]=num+1;
        }else
          s->lengthlist[i]=0;
      }
    }else{
      /* all entries used; no tagging */
      for(i=0;i<s->entries;i++){
        long num=oggpack_read(opb,5);
        if(num==-1)goto _eofout;
        s->lengthlist[i]=num+1;
      }
    }

    break;
  }
  case 1:
    /* ordered */
    {
      long length=oggpack_read(opb,5)+1;
      if(length==0)goto _eofout;
      s->lengthlist=(char*)_ogg_malloc(sizeof(*s->lengthlist)*s->entries);

      for(i=0;i<s->entries;){
        long num=oggpack_read(opb,ov_ilog(s->entries-i));
        if(num==-1)goto _eofout;
        if(length>32 || num>s->entries-i ||
           (num>0 && (num-1)>>(length-1)>1)){
          goto _errout;
        }
        if(length>32)goto _errout;
        for(j=0;j<num;j++,i++)
          s->lengthlist[i]=length;
        length++;
      }
    }
    break;
  default:
    /* EOF */
    goto _eofout;
  }

  /* Do we have a mapping to unpack? */
  switch((s->maptype=oggpack_read(opb,4))){
  case 0:
    /* no mapping */
    break;
  case 1: case 2:
    /* implicitly populated value mapping */
    /* explicitly populated value mapping */

    s->q_min=oggpack_read(opb,32);
    s->q_delta=oggpack_read(opb,32);
    s->q_quant=oggpack_read(opb,4)+1;
    s->q_sequencep=oggpack_read(opb,1);
    if(s->q_sequencep==-1)goto _eofout;

    {
      int quantvals=0;
      switch(s->maptype){
      case 1:
        quantvals=(s->dim==0?0:_book_maptype1_quantvals(s));
        break;
      case 2:
        quantvals=s->entries*s->dim;
        break;
      }

      /* quantized values */
      if(((quantvals*s->q_quant+7)>>3)>opb->storage-oggpack_bytes(opb))
        goto _eofout;
      s->quantlist=(long*)_ogg_malloc(sizeof(*s->quantlist)*quantvals);
      for(i=0;i<quantvals;i++)
        s->quantlist[i]=oggpack_read(opb,s->q_quant);

      if(quantvals&&s->quantlist[quantvals-1]==-1)goto _eofout;
    }
    break;
  default:
    goto _errout;
  }

  /* all set */
  return(s);

 _errout:
 _eofout:
  vorbis_staticbook_destroy(s);
  return(NULL);
}

/* returns the number of bits ************************************************/
int vorbis_book_encode(codebook *book, int a, oggpack_buffer *b){
  if(a<0 || a>=book->c->entries)return(0);
  oggpack_write(b,book->codelist[a],book->c->lengthlist[a]);
  return(book->c->lengthlist[a]);
}

/* the 'eliminate the decode tree' optimization actually requires the
   codewords to be MSb first, not LSb.  This is an annoying inelegancy
   (and one of the first places where carefully thought out design
   turned out to be wrong; Vorbis II and future Ogg codecs should go
   to an MSb bitpacker), but not actually the huge hit it appears to
   be.  The first-stage decode table catches most words so that
   _bitreverse is not in the main execution path. */

static ogg_uint32_t _bitreverse(ogg_uint32_t x){
  x=    ((x>>16)&0x0000ffff) | ((x<<16)&0xffff0000);
  x=    ((x>> 8)&0x00ff00ff) | ((x<< 8)&0xff00ff00);
  x=    ((x>> 4)&0x0f0f0f0f) | ((x<< 4)&0xf0f0f0f0);
  x=    ((x>> 2)&0x33333333) | ((x<< 2)&0xcccccccc);
  return((x>> 1)&0x55555555) | ((x<< 1)&0xaaaaaaaa);
}

STIN long decode_packed_entry_number(codebook *book, oggpack_buffer *b){
  int  read=book->dec_maxlength;
  long lo,hi;
  long lok = oggpack_look(b,book->dec_firsttablen);

  if (lok >= 0) {
    long entry = book->dec_firsttable[lok];
    if(entry&0x80000000UL){
      lo=(entry>>15)&0x7fff;
      hi=book->used_entries-(entry&0x7fff);
    }else{
      oggpack_adv(b, book->dec_codelengths[entry-1]);
      return(entry-1);
    }
  }else{
    lo=0;
    hi=book->used_entries;
  }

  /* Single entry codebooks use a firsttablen of 1 and a
     dec_maxlength of 1.  If a single-entry codebook gets here (due to
     failure to read one bit above), the next look attempt will also
     fail and we'll correctly kick out instead of trying to walk the
     underformed tree */

  lok = oggpack_look(b, read);

  while(lok<0 && read>1)
    lok = oggpack_look(b, --read);
  if(lok<0)return -1;

  /* bisect search for the codeword in the ordered list */
  {
    ogg_uint32_t testword=_bitreverse((ogg_uint32_t)lok);

    while(hi-lo>1){
      long p=(hi-lo)>>1;
      long test=book->codelist[lo+p]>testword;
      lo+=p&(test-1);
      hi-=p&(-test);
      }

    if(book->dec_codelengths[lo]<=read){
      oggpack_adv(b, book->dec_codelengths[lo]);
      return(lo);
    }
  }

  oggpack_adv(b, read);

  return(-1);
}

/* Decode side is specced and easier, because we don't need to find
   matches using different criteria; we simply read and map.  There are
   two things we need to do 'depending':

   We may need to support interleave.  We don't really, but it's
   convenient to do it here rather than rebuild the vector later.

   Cascades may be additive or multiplicitive; this is not inherent in
   the codebook, but set in the code using the codebook.  Like
   interleaving, it's easiest to do it here.
   addmul==0 -> declarative (set the value)
   addmul==1 -> additive
   addmul==2 -> multiplicitive */

/* returns the [original, not compacted] entry number or -1 on eof *********/
long vorbis_book_decode(codebook *book, oggpack_buffer *b){
  if(book->used_entries>0){
    long packed_entry=decode_packed_entry_number(book,b);
    if(packed_entry>=0)
      return(book->dec_index[packed_entry]);
  }

  /* if there's no dec_index, the codebook unpacking isn't collapsed */
  return(-1);
}

/* returns 0 on OK or -1 on eof *************************************/
/* decode vector / dim granularity gaurding is done in the upper layer */
long vorbis_book_decodevs_add(codebook *book,float *a,oggpack_buffer *b,int n){
  if(book->used_entries>0){
    int step=n/book->dim;
    long *entry = (long*)alloca(sizeof(*entry)*step);
    float **t = (float**)alloca(sizeof(*t)*step);
    int i,j,o;

    for (i = 0; i < step; i++) {
      entry[i]=decode_packed_entry_number(book,b);
      if(entry[i]==-1)return(-1);
      t[i] = book->valuelist+entry[i]*book->dim;
    }
    for(i=0,o=0;i<book->dim;i++,o+=step)
      for (j=0;o+j<n && j<step;j++)
        a[o+j]+=t[j][i];
  }
  return(0);
}

/* decode vector / dim granularity gaurding is done in the upper layer */
long vorbis_book_decodev_add(codebook *book,float *a,oggpack_buffer *b,int n){
  if(book->used_entries>0){
    int i,j,entry;
    float *t;

    for(i=0;i<n;){
      entry = decode_packed_entry_number(book,b);
      if(entry==-1)return(-1);
      t     = book->valuelist+entry*book->dim;
      for(j=0;i<n && j<book->dim;)
        a[i++]+=t[j++];
    }
  }
  return(0);
}

/* unlike the others, we guard against n not being an integer number
   of <dim> internally rather than in the upper layer (called only by
   floor0) */
long vorbis_book_decodev_set(codebook *book,float *a,oggpack_buffer *b,int n){
  if(book->used_entries>0){
    int i,j,entry;
    float *t;

    for(i=0;i<n;){
      entry = decode_packed_entry_number(book,b);
      if(entry==-1)return(-1);
      t     = book->valuelist+entry*book->dim;
      for (j=0;i<n && j<book->dim;){
        a[i++]=t[j++];
      }
    }
  }else{
    int i;

    for(i=0;i<n;){
      a[i++]=0.f;
    }
  }
  return(0);
}

long vorbis_book_decodevv_add(codebook *book,float **a,long offset,int ch,
                              oggpack_buffer *b,int n){

  long i,j,entry;
  int chptr=0;
  if(book->used_entries>0){
    int m=(offset+n)/ch;
    for(i=offset/ch;i<m;){
      entry = decode_packed_entry_number(book,b);
      if(entry==-1)return(-1);
      {
        const float *t = book->valuelist+entry*book->dim;
        for (j=0;i<m && j<book->dim;j++){
          a[chptr++][i]+=t[j];
          if(chptr==ch){
            chptr=0;
            i++;
          }
        }
      }
    }
  }
  return(0);
}
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2015             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: basic shared codebook operations

 ********************************************************************/

#include <stdlib.h>
#include <limits.h>
#include <math.h>
#include <string.h>
/*#include <ogg/ogg.h>*/
/*#include "os.h"*/
/*#include "misc.h"*/
/*#include "vorbis/codec.h"*/
/*#include "codebook.h"*/
/*#include "scales.h"*/

/**** pack/unpack helpers ******************************************/

int ov_ilog(ogg_uint32_t v){
  int ret;
  for(ret=0;v;ret++)v>>=1;
  return ret;
}

/* 32 bit float (not IEEE; nonnormalized mantissa +
   biased exponent) : neeeeeee eeemmmmm mmmmmmmm mmmmmmmm
   Why not IEEE?  It's just not that important here. */

#define VQ_FEXP 10
#define VQ_FMAN 21
#define VQ_FEXP_BIAS 768 /* bias toward values smaller than 1. */

/* doesn't currently guard under/overflow */
long _float32_pack(float val){
  int sign=0;
  long exp;
  long mant;
  if(val<0){
    sign=0x80000000;
    val= -val;
  }
  exp= floor(log(val)/log(2.f)+.001); /* +epsilon */
  mant=rint(ldexp(val,(VQ_FMAN-1)-exp));
  exp=(exp+VQ_FEXP_BIAS)<<VQ_FMAN;

  return(sign|exp|mant);
}

float _float32_unpack(long val){
  double mant=val&0x1fffff;
  int    sign=val&0x80000000;
  long   exp =(val&0x7fe00000L)>>VQ_FMAN;
  if(sign)mant= -mant;
  exp=exp-(VQ_FMAN-1)-VQ_FEXP_BIAS;
  /* clamp excessive exponent values */
  if (exp>63){
    exp=63;
  }
  if (exp<-63){
    exp=-63;
  }
  return(ldexp(mant,exp));
}

/* given a list of word lengths, generate a list of codewords.  Works
   for length ordered or unordered, always assigns the lowest valued
   codewords first.  Extended to handle unused entries (length 0) */
ogg_uint32_t *_make_words(char *l,long n,long sparsecount){
  long i,j,count=0;
  ogg_uint32_t marker[33];
  ogg_uint32_t *r=(ogg_uint32_t*)_ogg_malloc((sparsecount?sparsecount:n)*sizeof(*r));
  memset(marker,0,sizeof(marker));

  for(i=0;i<n;i++){
    long length=l[i];
    if(length>0){
      ogg_uint32_t entry=marker[length];

      /* when we claim a node for an entry, we also claim the nodes
         below it (pruning off the imagined tree that may have dangled
         from it) as well as blocking the use of any nodes directly
         above for leaves */

      /* update ourself */
      if(length<32 && (entry>>length)){
        /* error condition; the lengths must specify an overpopulated tree */
        _ogg_free(r);
        return(NULL);
      }
      r[count++]=entry;

      /* Look to see if the next shorter marker points to the node
         above. if so, update it and repeat.  */
      {
        for(j=length;j>0;j--){

          if(marker[j]&1){
            /* have to jump branches */
            if(j==1)
              marker[1]++;
            else
              marker[j]=marker[j-1]<<1;
            break; /* invariant says next upper marker would already
                      have been moved if it was on the same path */
          }
          marker[j]++;
        }
      }

      /* prune the tree; the implicit invariant says all the longer
         markers were dangling from our just-taken node.  Dangle them
         from our *new* node. */
      for(j=length+1;j<33;j++)
        if((marker[j]>>1) == entry){
          entry=marker[j];
          marker[j]=marker[j-1]<<1;
        }else
          break;
    }else
      if(sparsecount==0)count++;
  }

  /* any underpopulated tree must be rejected. */
  /* Single-entry codebooks are a retconned extension to the spec.
     They have a single codeword '0' of length 1 that results in an
     underpopulated tree.  Shield that case from the underformed tree check. */
  if(!(count==1 && marker[2]==2)){
    for(i=1;i<33;i++)
      if(marker[i] & (0xffffffffUL>>(32-i))){
        _ogg_free(r);
        return(NULL);
      }
  }

  /* bitreverse the words because our bitwise packer/unpacker is LSb
     endian */
  for(i=0,count=0;i<n;i++){
    ogg_uint32_t temp=0;
    for(j=0;j<l[i];j++){
      temp<<=1;
      temp|=(r[count]>>j)&1;
    }

    if(sparsecount){
      if(l[i])
        r[count++]=temp;
    }else
      r[count++]=temp;
  }

  return(r);
}

/* there might be a straightforward one-line way to do the below
   that's portable and totally safe against roundoff, but I haven't
   thought of it.  Therefore, we opt on the side of caution */
long _book_maptype1_quantvals(const static_codebook *b){
  long vals;
  if(b->entries<1){
    return(0);
  }
  vals=floor(pow((float)b->entries,1.f/b->dim));

  /* the above *should* be reliable, but we'll not assume that FP is
     ever reliable when bitstream sync is at stake; verify via integer
     means that vals really is the greatest value of dim for which
     vals^b->bim <= b->entries */
  /* treat the above as an initial guess */
  if(vals<1){
    vals=1;
  }
  while(1){
    long acc=1;
    long acc1=1;
    int i;
    for(i=0;i<b->dim;i++){
      if(b->entries/vals<acc)break;
      acc*=vals;
      if(LONG_MAX/(vals+1)<acc1)acc1=LONG_MAX;
      else acc1*=vals+1;
    }
    if(i>=b->dim && acc<=b->entries && acc1>b->entries){
      return(vals);
    }else{
      if(i<b->dim || acc>b->entries){
        vals--;
      }else{
        vals++;
      }
    }
  }
}

/* unpack the quantized list of values for encode/decode ***********/
/* we need to deal with two map types: in map type 1, the values are
   generated algorithmically (each column of the vector counts through
   the values in the quant vector). in map type 2, all the values came
   in in an explicit list.  Both value lists must be unpacked */
float *_book_unquantize(const static_codebook *b,int n,int *sparsemap){
  long j,k,count=0;
  if(b->maptype==1 || b->maptype==2){
    int quantvals;
    float mindel=_float32_unpack(b->q_min);
    float delta=_float32_unpack(b->q_delta);
    float *r=(float*)_ogg_calloc(n*b->dim,sizeof(*r));

    /* maptype 1 and 2 both use a quantized value vector, but
       different sizes */
    switch(b->maptype){
    case 1:
      /* most of the time, entries%dimensions == 0, but we need to be
         well defined.  We define that the possible vales at each
         scalar is values == entries/dim.  If entries%dim != 0, we'll
         have 'too few' values (values*dim<entries), which means that
         we'll have 'left over' entries; left over entries use zeroed
         values (and are wasted).  So don't generate codebooks like
         that */
      quantvals=_book_maptype1_quantvals(b);
      for(j=0;j<b->entries;j++){
        if((sparsemap && b->lengthlist[j]) || !sparsemap){
          float last=0.f;
          int indexdiv=1;
          for(k=0;k<b->dim;k++){
            int index= (j/indexdiv)%quantvals;
            float val=b->quantlist[index];
            val=fabs(val)*delta+mindel+last;
            if(b->q_sequencep)last=val;
            if(sparsemap)
              r[sparsemap[count]*b->dim+k]=val;
            else
              r[count*b->dim+k]=val;
            indexdiv*=quantvals;
          }
          count++;
        }

      }
      break;
    case 2:
      for(j=0;j<b->entries;j++){
        if((sparsemap && b->lengthlist[j]) || !sparsemap){
          float last=0.f;

          for(k=0;k<b->dim;k++){
            float val=b->quantlist[j*b->dim+k];
            val=fabs(val)*delta+mindel+last;
            if(b->q_sequencep)last=val;
            if(sparsemap)
              r[sparsemap[count]*b->dim+k]=val;
            else
              r[count*b->dim+k]=val;
          }
          count++;
        }
      }
      break;
    }

    return(r);
  }
  return(NULL);
}

void vorbis_staticbook_destroy(static_codebook *b){
  if(b->allocedp){
    if(b->quantlist)_ogg_free(b->quantlist);
    if(b->lengthlist)_ogg_free(b->lengthlist);
    memset(b,0,sizeof(*b));
    _ogg_free(b);
  } /* otherwise, it is in static memory */
}

void vorbis_book_clear(codebook *b){
  /* static book is not cleared; we're likely called on the lookup and
     the static codebook belongs to the info struct */
  if(b->valuelist)_ogg_free(b->valuelist);
  if(b->codelist)_ogg_free(b->codelist);

  if(b->dec_index)_ogg_free(b->dec_index);
  if(b->dec_codelengths)_ogg_free(b->dec_codelengths);
  if(b->dec_firsttable)_ogg_free(b->dec_firsttable);

  memset(b,0,sizeof(*b));
}

int vorbis_book_init_encode(codebook *c,const static_codebook *s){

  memset(c,0,sizeof(*c));
  c->c=s;
  c->entries=s->entries;
  c->used_entries=s->entries;
  c->dim=s->dim;
  c->codelist=_make_words(s->lengthlist,s->entries,0);
  /* c->valuelist=_book_unquantize(s,s->entries,NULL); */
  c->quantvals=_book_maptype1_quantvals(s);
  c->minval=(int)rint(_float32_unpack(s->q_min));
  c->delta=(int)rint(_float32_unpack(s->q_delta));

  return(0);
}

static ogg_uint32_t bitreverse(ogg_uint32_t x){
  x=    ((x>>16)&0x0000ffffUL) | ((x<<16)&0xffff0000UL);
  x=    ((x>> 8)&0x00ff00ffUL) | ((x<< 8)&0xff00ff00UL);
  x=    ((x>> 4)&0x0f0f0f0fUL) | ((x<< 4)&0xf0f0f0f0UL);
  x=    ((x>> 2)&0x33333333UL) | ((x<< 2)&0xccccccccUL);
  return((x>> 1)&0x55555555UL) | ((x<< 1)&0xaaaaaaaaUL);
}

static int sort32a(const void *a,const void *b){
  return ( **(ogg_uint32_t **)a>**(ogg_uint32_t **)b)-
    ( **(ogg_uint32_t **)a<**(ogg_uint32_t **)b);
}

/* decode codebook arrangement is more heavily optimized than encode */
int vorbis_book_init_decode(codebook *c,const static_codebook *s){
  int i,j,n=0,tabn;
  int *sortindex;

  memset(c,0,sizeof(*c));

  /* count actually used entries and find max length */
  for(i=0;i<s->entries;i++)
    if(s->lengthlist[i]>0)
      n++;

  c->entries=s->entries;
  c->used_entries=n;
  c->dim=s->dim;

  if(n>0){
    /* two different remappings go on here.

    First, we collapse the likely sparse codebook down only to
    actually represented values/words.  This collapsing needs to be
    indexed as map-valueless books are used to encode original entry
    positions as integers.

    Second, we reorder all vectors, including the entry index above,
    by sorted bitreversed codeword to allow treeless decode. */

    /* perform sort */
    ogg_uint32_t *codes=_make_words(s->lengthlist,s->entries,c->used_entries);
    ogg_uint32_t **codep=(ogg_uint32_t**)alloca(sizeof(*codep)*n);

    if(codes==NULL)goto err_out;

    for(i=0;i<n;i++){
      codes[i]=bitreverse(codes[i]);
      codep[i]=codes+i;
    }

    qsort(codep,n,sizeof(*codep),sort32a);

    sortindex=(int*)alloca(n*sizeof(*sortindex));
    c->codelist=(ogg_uint32_t*)_ogg_malloc(n*sizeof(*c->codelist));
    /* the index is a reverse index */
    for(i=0;i<n;i++){
      int position=codep[i]-codes;
      sortindex[position]=i;
    }

    for(i=0;i<n;i++)
      c->codelist[sortindex[i]]=codes[i];
    _ogg_free(codes);

    c->valuelist=_book_unquantize(s,n,sortindex);
    c->dec_index=(int*)_ogg_malloc(n*sizeof(*c->dec_index));

    for(n=0,i=0;i<s->entries;i++)
      if(s->lengthlist[i]>0)
        c->dec_index[sortindex[n++]]=i;

    c->dec_codelengths=(char*)_ogg_malloc(n*sizeof(*c->dec_codelengths));
    c->dec_maxlength=0;
    for(n=0,i=0;i<s->entries;i++)
      if(s->lengthlist[i]>0){
        c->dec_codelengths[sortindex[n++]]=s->lengthlist[i];
        if(s->lengthlist[i]>c->dec_maxlength)
          c->dec_maxlength=s->lengthlist[i];
      }

    if(n==1 && c->dec_maxlength==1){
      /* special case the 'single entry codebook' with a single bit
       fastpath table (that always returns entry 0 )in order to use
       unmodified decode paths. */
      c->dec_firsttablen=1;
      c->dec_firsttable=(ogg_uint32_t*)_ogg_calloc(2,sizeof(*c->dec_firsttable));
      c->dec_firsttable[0]=c->dec_firsttable[1]=1;

    }else{
      c->dec_firsttablen=ov_ilog(c->used_entries)-4; /* this is magic */
      if(c->dec_firsttablen<5)c->dec_firsttablen=5;
      if(c->dec_firsttablen>8)c->dec_firsttablen=8;

      tabn=1<<c->dec_firsttablen;
      c->dec_firsttable=(ogg_uint32_t*)_ogg_calloc(tabn,sizeof(*c->dec_firsttable));

      for(i=0;i<n;i++){
        if(c->dec_codelengths[i]<=c->dec_firsttablen){
          ogg_uint32_t orig=bitreverse(c->codelist[i]);
          for(j=0;j<(1<<(c->dec_firsttablen-c->dec_codelengths[i]));j++)
            c->dec_firsttable[orig|(j<<c->dec_codelengths[i])]=i+1;
        }
      }

      /* now fill in 'unused' entries in the firsttable with hi/lo search
         hints for the non-direct-hits */
      {
        ogg_uint32_t mask=0xfffffffeUL<<(31-c->dec_firsttablen);
        long lo=0,hi=0;

        for(i=0;i<tabn;i++){
          ogg_uint32_t word=i<<(32-c->dec_firsttablen);
          if(c->dec_firsttable[bitreverse(word)]==0){
            while((lo+1)<n && c->codelist[lo+1]<=word)lo++;
            while(    hi<n && word>=(c->codelist[hi]&mask))hi++;

            /* we only actually have 15 bits per hint to play with here.
               In order to overflow gracefully (nothing breaks, efficiency
               just drops), encode as the difference from the extremes. */
            {
              unsigned long loval=lo;
              unsigned long hival=n-hi;

              if(loval>0x7fff)loval=0x7fff;
              if(hival>0x7fff)hival=0x7fff;
              c->dec_firsttable[bitreverse(word)]=
                0x80000000UL | (loval<<15) | hival;
            }
          }
        }
      }
    }
  }

  return(0);
 err_out:
  vorbis_book_clear(c);
  return(-1);
}

long vorbis_book_codeword(codebook *book,int entry){
  if(book->c) /* only use with encode; decode optimizations are
                 allowed to break this */
    return book->codelist[entry];
  return -1;
}

long vorbis_book_codelen(codebook *book,int entry){
  if(book->c) /* only use with encode; decode optimizations are
                 allowed to break this */
    return book->c->lengthlist[entry];
  return -1;
}

#ifdef _V_SELFTEST

/* Unit tests of the dequantizer; this stuff will be OK
   cross-platform, I simply want to be sure that special mapping cases
   actually work properly; a bug could go unnoticed for a while */

#include <stdio.h>

/* cases:

   no mapping
   full, explicit mapping
   algorithmic mapping

   nonsequential
   sequential
*/

static long full_quantlist1[]={0,1,2,3,    4,5,6,7, 8,3,6,1};
static long partial_quantlist1[]={0,7,2};

/* no mapping */
static_codebook test1={
  4,16,
  NULL,
  0,
  0,0,0,0,
  NULL,
  0
};
static float *test1_result=NULL;

/* linear, full mapping, nonsequential */
static_codebook test2={
  4,3,
  NULL,
  2,
  -533200896,1611661312,4,0,
  full_quantlist1,
  0
};
static float test2_result[]={-3,-2,-1,0, 1,2,3,4, 5,0,3,-2};

/* linear, full mapping, sequential */
static_codebook test3={
  4,3,
  NULL,
  2,
  -533200896,1611661312,4,1,
  full_quantlist1,
  0
};
static float test3_result[]={-3,-5,-6,-6, 1,3,6,10, 5,5,8,6};

/* linear, algorithmic mapping, nonsequential */
static_codebook test4={
  3,27,
  NULL,
  1,
  -533200896,1611661312,4,0,
  partial_quantlist1,
  0
};
static float test4_result[]={-3,-3,-3, 4,-3,-3, -1,-3,-3,
                              -3, 4,-3, 4, 4,-3, -1, 4,-3,
                              -3,-1,-3, 4,-1,-3, -1,-1,-3,
                              -3,-3, 4, 4,-3, 4, -1,-3, 4,
                              -3, 4, 4, 4, 4, 4, -1, 4, 4,
                              -3,-1, 4, 4,-1, 4, -1,-1, 4,
                              -3,-3,-1, 4,-3,-1, -1,-3,-1,
                              -3, 4,-1, 4, 4,-1, -1, 4,-1,
                              -3,-1,-1, 4,-1,-1, -1,-1,-1};

/* linear, algorithmic mapping, sequential */
static_codebook test5={
  3,27,
  NULL,
  1,
  -533200896,1611661312,4,1,
  partial_quantlist1,
  0
};
static float test5_result[]={-3,-6,-9, 4, 1,-2, -1,-4,-7,
                              -3, 1,-2, 4, 8, 5, -1, 3, 0,
                              -3,-4,-7, 4, 3, 0, -1,-2,-5,
                              -3,-6,-2, 4, 1, 5, -1,-4, 0,
                              -3, 1, 5, 4, 8,12, -1, 3, 7,
                              -3,-4, 0, 4, 3, 7, -1,-2, 2,
                              -3,-6,-7, 4, 1, 0, -1,-4,-5,
                              -3, 1, 0, 4, 8, 7, -1, 3, 2,
                              -3,-4,-5, 4, 3, 2, -1,-2,-3};

void run_test(static_codebook *b,float *comp){
  float *out=_book_unquantize(b,b->entries,NULL);
  int i;

  if(comp){
    if(!out){
      fprintf(stderr,"_book_unquantize incorrectly returned NULL\n");
      exit(1);
    }

    for(i=0;i<b->entries*b->dim;i++)
      if(fabs(out[i]-comp[i])>.0001){
        fprintf(stderr,"disagreement in unquantized and reference data:\n"
                "position %d, %g != %g\n",i,out[i],comp[i]);
        exit(1);
      }

  }else{
    if(out){
      fprintf(stderr,"_book_unquantize returned a value array: \n"
              " correct result should have been NULL\n");
      exit(1);
    }
  }
  free(out);
}

int main(){
  /* run the nine dequant tests, and compare to the hand-rolled results */
  fprintf(stderr,"Dequant test 1... ");
  run_test(&test1,test1_result);
  fprintf(stderr,"OK\nDequant test 2... ");
  run_test(&test2,test2_result);
  fprintf(stderr,"OK\nDequant test 3... ");
  run_test(&test3,test3_result);
  fprintf(stderr,"OK\nDequant test 4... ");
  run_test(&test4,test4_result);
  fprintf(stderr,"OK\nDequant test 5... ");
  run_test(&test5,test5_result);
  fprintf(stderr,"OK\n\n");

  return(0);
}

#endif
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

  function: lookup based functions

 ********************************************************************/

#include <math.h>
/*#include "lookup.h"*/
/*#include "lookup_data.h"*/
/*#include "os.h"*/
/*#include "misc.h"*/

#ifdef FLOAT_LOOKUP

/* interpolated lookup based cos function, domain 0 to PI only */
float vorbis_coslook(float a){
  double d=a*(.31830989*(float)COS_LOOKUP_SZ);
  int i=vorbis_ftoi(d-.5);

  return COS_LOOKUP[i]+ (d-i)*(COS_LOOKUP[i+1]-COS_LOOKUP[i]);
}

/* interpolated 1./sqrt(p) where .5 <= p < 1. */
float vorbis_invsqlook(float a){
  double d=a*(2.f*(float)INVSQ_LOOKUP_SZ)-(float)INVSQ_LOOKUP_SZ;
  int i=vorbis_ftoi(d-.5f);
  return INVSQ_LOOKUP[i]+ (d-i)*(INVSQ_LOOKUP[i+1]-INVSQ_LOOKUP[i]);
}

/* interpolated 1./sqrt(p) where .5 <= p < 1. */
float vorbis_invsq2explook(int a){
  return INVSQ2EXP_LOOKUP[a-INVSQ2EXP_LOOKUP_MIN];
}

#include <stdio.h>
/* interpolated lookup based fromdB function, domain -140dB to 0dB only */
float vorbis_fromdBlook(float a){
  int i=vorbis_ftoi(a*((float)(-(1<<FROMdB2_SHIFT)))-.5f);
  return (i<0)?1.f:
    ((i>=(FROMdB_LOOKUP_SZ<<FROMdB_SHIFT))?0.f:
     FROMdB_LOOKUP[i>>FROMdB_SHIFT]*FROMdB2_LOOKUP[i&FROMdB2_MASK]);
}

#endif

#ifdef INT_LOOKUP
/* interpolated 1./sqrt(p) where .5 <= a < 1. (.100000... to .111111...) in
   16.16 format

   returns in m.8 format */
long vorbis_invsqlook_i(long a,long e){
  long i=(a&0x7fff)>>(INVSQ_LOOKUP_I_SHIFT-1);
  long d=(a&INVSQ_LOOKUP_I_MASK)<<(16-INVSQ_LOOKUP_I_SHIFT); /*  0.16 */
  long val=INVSQ_LOOKUP_I[i]-                                /*  1.16 */
    (((INVSQ_LOOKUP_I[i]-INVSQ_LOOKUP_I[i+1])*               /*  0.16 */
      d)>>16);                                               /* result 1.16 */

  e+=32;
  if(e&1)val=(val*5792)>>13; /* multiply val by 1/sqrt(2) */
  e=(e>>1)-8;

  return(val>>e);
}

/* interpolated lookup based fromdB function, domain -140dB to 0dB only */
/* a is in n.12 format */
float vorbis_fromdBlook_i(long a){
  int i=(-a)>>(12-FROMdB2_SHIFT);
  return (i<0)?1.f:
    ((i>=(FROMdB_LOOKUP_SZ<<FROMdB_SHIFT))?0.f:
     FROMdB_LOOKUP[i>>FROMdB_SHIFT]*FROMdB2_LOOKUP[i&FROMdB2_MASK]);
}

/* interpolated lookup based cos function, domain 0 to PI only */
/* a is in 0.16 format, where 0==0, 2^^16-1==PI, return 0.14 */
long vorbis_coslook_i(long a){
  int i=a>>COS_LOOKUP_I_SHIFT;
  int d=a&COS_LOOKUP_I_MASK;
  return COS_LOOKUP_I[i]- ((d*(COS_LOOKUP_I[i]-COS_LOOKUP_I[i+1]))>>
                           COS_LOOKUP_I_SHIFT);
}

#endif
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: bitrate tracking and management

 ********************************************************************/

#include <stdlib.h>
#include <string.h>
#include <math.h>
/*#include <ogg/ogg.h>*/
/*#include "vorbis/codec.h"*/
/*#include "codec_internal.h"*/
/*#include "os.h"*/
/*#include "misc.h"*/
/*#include "bitrate.h"*/

/* compute bitrate tracking setup  */
void vorbis_bitrate_init(vorbis_info *vi,bitrate_manager_state *bm){
  codec_setup_info *ci=(codec_setup_info*)vi->codec_setup;
  bitrate_manager_info *bi=&ci->bi;

  memset(bm,0,sizeof(*bm));

  if(bi && (bi->reservoir_bits>0)){
    long ratesamples=vi->rate;
    int  halfsamples=ci->blocksizes[0]>>1;

    bm->short_per_long=ci->blocksizes[1]/ci->blocksizes[0];
    bm->managed=1;

    bm->avg_bitsper= rint(1.*bi->avg_rate*halfsamples/ratesamples);
    bm->min_bitsper= rint(1.*bi->min_rate*halfsamples/ratesamples);
    bm->max_bitsper= rint(1.*bi->max_rate*halfsamples/ratesamples);

    bm->avgfloat=PACKETBLOBS/2;

    /* not a necessary fix, but one that leads to a more balanced
       typical initialization */
    {
      long desired_fill=bi->reservoir_bits*bi->reservoir_bias;
      bm->minmax_reservoir=desired_fill;
      bm->avg_reservoir=desired_fill;
    }

  }
}

void vorbis_bitrate_clear(bitrate_manager_state *bm){
  memset(bm,0,sizeof(*bm));
  return;
}

int vorbis_bitrate_managed(vorbis_block *vb){
  vorbis_dsp_state      *vd=vb->vd;
  private_state         *b=(private_state*)vd->backend_state;
  bitrate_manager_state *bm=&b->bms;

  if(bm && bm->managed)return(1);
  return(0);
}

/* finish taking in the block we just processed */
int vorbis_bitrate_addblock(vorbis_block *vb){
  vorbis_block_internal *vbi=(vorbis_block_internal*)vb->internal;
  vorbis_dsp_state      *vd=vb->vd;
  private_state         *b=(private_state*)vd->backend_state;
  bitrate_manager_state *bm=&b->bms;
  vorbis_info           *vi=vd->vi;
  codec_setup_info      *ci=(codec_setup_info*)vi->codec_setup;
  bitrate_manager_info  *bi=&ci->bi;

  int  choice=rint(bm->avgfloat);
  long this_bits=oggpack_bytes(vbi->packetblob[choice])*8;
  long min_target_bits=(vb->W?bm->min_bitsper*bm->short_per_long:bm->min_bitsper);
  long max_target_bits=(vb->W?bm->max_bitsper*bm->short_per_long:bm->max_bitsper);
  int  samples=ci->blocksizes[vb->W]>>1;
  long desired_fill=bi->reservoir_bits*bi->reservoir_bias;
  if(!bm->managed){
    /* not a bitrate managed stream, but for API simplicity, we'll
       buffer the packet to keep the code path clean */

    if(bm->vb)return(-1); /* one has been submitted without
                             being claimed */
    bm->vb=vb;
    return(0);
  }

  bm->vb=vb;

  /* look ahead for avg floater */
  if(bm->avg_bitsper>0){
    double slew=0.;
    long avg_target_bits=(vb->W?bm->avg_bitsper*bm->short_per_long:bm->avg_bitsper);
    double slewlimit= 15./bi->slew_damp;

    /* choosing a new floater:
       if we're over target, we slew down
       if we're under target, we slew up

       choose slew as follows: look through packetblobs of this frame
       and set slew as the first in the appropriate direction that
       gives us the slew we want.  This may mean no slew if delta is
       already favorable.

       Then limit slew to slew max */

    if(bm->avg_reservoir+(this_bits-avg_target_bits)>desired_fill){
      while(choice>0 && this_bits>avg_target_bits &&
            bm->avg_reservoir+(this_bits-avg_target_bits)>desired_fill){
        choice--;
        this_bits=oggpack_bytes(vbi->packetblob[choice])*8;
      }
    }else if(bm->avg_reservoir+(this_bits-avg_target_bits)<desired_fill){
      while(choice+1<PACKETBLOBS && this_bits<avg_target_bits &&
            bm->avg_reservoir+(this_bits-avg_target_bits)<desired_fill){
        choice++;
        this_bits=oggpack_bytes(vbi->packetblob[choice])*8;
      }
    }

    slew=rint(choice-bm->avgfloat)/samples*vi->rate;
    if(slew<-slewlimit)slew=-slewlimit;
    if(slew>slewlimit)slew=slewlimit;
    choice=rint(bm->avgfloat+= slew/vi->rate*samples);
    this_bits=oggpack_bytes(vbi->packetblob[choice])*8;
  }



  /* enforce min(if used) on the current floater (if used) */
  if(bm->min_bitsper>0){
    /* do we need to force the bitrate up? */
    if(this_bits<min_target_bits){
      while(bm->minmax_reservoir-(min_target_bits-this_bits)<0){
        choice++;
        if(choice>=PACKETBLOBS)break;
        this_bits=oggpack_bytes(vbi->packetblob[choice])*8;
      }
    }
  }

  /* enforce max (if used) on the current floater (if used) */
  if(bm->max_bitsper>0){
    /* do we need to force the bitrate down? */
    if(this_bits>max_target_bits){
      while(bm->minmax_reservoir+(this_bits-max_target_bits)>bi->reservoir_bits){
        choice--;
        if(choice<0)break;
        this_bits=oggpack_bytes(vbi->packetblob[choice])*8;
      }
    }
  }

  /* Choice of packetblobs now made based on floater, and min/max
     requirements. Now boundary check extreme choices */

  if(choice<0){
    /* choosing a smaller packetblob is insufficient to trim bitrate.
       frame will need to be truncated */
    long maxsize=(max_target_bits+(bi->reservoir_bits-bm->minmax_reservoir))/8;
    bm->choice=choice=0;

    if(oggpack_bytes(vbi->packetblob[choice])>maxsize){

      oggpack_writetrunc(vbi->packetblob[choice],maxsize*8);
      this_bits=oggpack_bytes(vbi->packetblob[choice])*8;
    }
  }else{
    long minsize=(min_target_bits-bm->minmax_reservoir+7)/8;
    if(choice>=PACKETBLOBS)
      choice=PACKETBLOBS-1;

    bm->choice=choice;

    /* prop up bitrate according to demand. pad this frame out with zeroes */
    minsize-=oggpack_bytes(vbi->packetblob[choice]);
    while(minsize-->0)oggpack_write(vbi->packetblob[choice],0,8);
    this_bits=oggpack_bytes(vbi->packetblob[choice])*8;

  }

  /* now we have the final packet and the final packet size.  Update statistics */
  /* min and max reservoir */
  if(bm->min_bitsper>0 || bm->max_bitsper>0){

    if(max_target_bits>0 && this_bits>max_target_bits){
      bm->minmax_reservoir+=(this_bits-max_target_bits);
    }else if(min_target_bits>0 && this_bits<min_target_bits){
      bm->minmax_reservoir+=(this_bits-min_target_bits);
    }else{
      /* inbetween; we want to take reservoir toward but not past desired_fill */
      if(bm->minmax_reservoir>desired_fill){
        if(max_target_bits>0){ /* logical bulletproofing against initialization state */
          bm->minmax_reservoir+=(this_bits-max_target_bits);
          if(bm->minmax_reservoir<desired_fill)bm->minmax_reservoir=desired_fill;
        }else{
          bm->minmax_reservoir=desired_fill;
        }
      }else{
        if(min_target_bits>0){ /* logical bulletproofing against initialization state */
          bm->minmax_reservoir+=(this_bits-min_target_bits);
          if(bm->minmax_reservoir>desired_fill)bm->minmax_reservoir=desired_fill;
        }else{
          bm->minmax_reservoir=desired_fill;
        }
      }
    }
  }

  /* avg reservoir */
  if(bm->avg_bitsper>0){
    long avg_target_bits=(vb->W?bm->avg_bitsper*bm->short_per_long:bm->avg_bitsper);
    bm->avg_reservoir+=this_bits-avg_target_bits;
  }

  return(0);
}

int vorbis_bitrate_flushpacket(vorbis_dsp_state *vd,ogg_packet *op){
  private_state         *b=(private_state*)vd->backend_state;
  bitrate_manager_state *bm=&b->bms;
  vorbis_block          *vb=bm->vb;
  int                    choice=PACKETBLOBS/2;
  if(!vb)return 0;

  if(op){
    vorbis_block_internal *vbi=(vorbis_block_internal*)vb->internal;

    if(vorbis_bitrate_managed(vb))
      choice=bm->choice;

    op->packet=oggpack_get_buffer(vbi->packetblob[choice]);
    op->bytes=oggpack_bytes(vbi->packetblob[choice]);
    op->b_o_s=0;
    op->e_o_s=vb->eofflag;
    op->granulepos=vb->granulepos;
    op->packetno=vb->sequence; /* for sake of completeness */
  }

  bm->vb=0;
  return(1);
}
/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2015             *
 * by the Xiph.Org Foundation https://xiph.org/                     *
 *                                                                  *
 ********************************************************************

 function: stdio-based convenience library for opening/seeking/decoding

 ********************************************************************/

#include <stdlib.h>
#include <stdio.h>
#include <errno.h>
#include <string.h>
#include <math.h>

/*#include "vorbis/codec.h"*/

/* we don't need or want the static callback symbols here */
#define OV_EXCLUDE_STATIC_CALLBACKS
/*#include "vorbis/vorbisfile.h"*/

/*#include "os.h"*/
/*#include "misc.h"*/

/* A 'chained bitstream' is a Vorbis bitstream that contains more than
   one logical bitstream arranged end to end (the only form of Ogg
   multiplexing allowed in a Vorbis bitstream; grouping [parallel
   multiplexing] is not allowed in Vorbis) */

/* A Vorbis file can be played beginning to end (streamed) without
   worrying ahead of time about chaining (see decoder_example.c).  If
   we have the whole file, however, and want random access
   (seeking/scrubbing) or desire to know the total length/time of a
   file, we need to account for the possibility of chaining. */

/* We can handle things a number of ways; we can determine the entire
   bitstream structure right off the bat, or find pieces on demand.
   This example determines and caches structure for the entire
   bitstream, but builds a virtual decoder on the fly when moving
   between links in the chain. */

/* There are also different ways to implement seeking.  Enough
   information exists in an Ogg bitstream to seek to
   sample-granularity positions in the output.  Or, one can seek by
   picking some portion of the stream roughly in the desired area if
   we only want coarse navigation through the stream. */

/*************************************************************************
 * Many, many internal helpers.  The intention is not to be confusing;
 * rampant duplication and monolithic function implementation would be
 * harder to understand anyway.  The high level functions are last.  Begin
 * grokking near the end of the file */

/* read a little more data from the file/pipe into the ogg_sync framer
*/
#define CHUNKSIZE 65536 /* greater-than-page-size granularity seeking */
#define READSIZE 2048 /* a smaller read size is needed for low-rate streaming. */

static long _get_data(OggVorbis_File *vf){
  errno=0;
  if(!(vf->callbacks.read_func))return(-1);
  if(vf->datasource){
    char *buffer=ogg_sync_buffer(&vf->oy,READSIZE);
    long bytes=(vf->callbacks.read_func)(buffer,1,READSIZE,vf->datasource);
    if(bytes>0)ogg_sync_wrote(&vf->oy,bytes);
    if(bytes==0 && errno)return(-1);
    return(bytes);
  }else
    return(0);
}

/* save a tiny smidge of verbosity to make the code more readable */
static int _seek_helper(OggVorbis_File *vf,ogg_int64_t offset){
  if(vf->datasource){
    /* only seek if the file position isn't already there */
    if(vf->offset != offset){
      if(!(vf->callbacks.seek_func)||
         (vf->callbacks.seek_func)(vf->datasource, offset, SEEK_SET) == -1)
        return OV_EREAD;
      vf->offset=offset;
      ogg_sync_reset(&vf->oy);
    }
  }else{
    /* shouldn't happen unless someone writes a broken callback */
    return OV_EFAULT;
  }
  return 0;
}

/* The read/seek functions track absolute position within the stream */

/* from the head of the stream, get the next page.  boundary specifies
   if the function is allowed to fetch more data from the stream (and
   how much) or only use internally buffered data.

   boundary: -1) unbounded search
              0) read no additional data; use cached only
              n) search for a new page beginning for n bytes

   return:   <0) did not find a page (OV_FALSE, OV_EOF, OV_EREAD)
              n) found a page at absolute offset n */

static ogg_int64_t _get_next_page(OggVorbis_File *vf,ogg_page *og,
                                  ogg_int64_t boundary){
  if(boundary>0)boundary+=vf->offset;
  while(1){
    long more;

    if(boundary>0 && vf->offset>=boundary)return(OV_FALSE);
    more=ogg_sync_pageseek(&vf->oy,og);

    if(more<0){
      /* skipped n bytes */
      vf->offset-=more;
    }else{
      if(more==0){
        /* send more paramedics */
        if(!boundary)return(OV_FALSE);
        {
          long ret=_get_data(vf);
          if(ret==0)return(OV_EOF);
          if(ret<0)return(OV_EREAD);
        }
      }else{
        /* got a page.  Return the offset at the page beginning,
           advance the internal offset past the page end */
        ogg_int64_t ret=vf->offset;
        vf->offset+=more;
        return(ret);

      }
    }
  }
}

/* find the latest page beginning before the passed in position. Much
   dirtier than the above as Ogg doesn't have any backward search
   linkage.  no 'readp' as it will certainly have to read. */
/* returns offset or OV_EREAD, OV_FAULT */
static ogg_int64_t _get_prev_page(OggVorbis_File *vf,ogg_int64_t begin,ogg_page *og){
  ogg_int64_t end = begin;
  ogg_int64_t ret;
  ogg_int64_t offset=-1;

  while(offset==-1){
    begin-=CHUNKSIZE;
    if(begin<0)
      begin=0;

    ret=_seek_helper(vf,begin);
    if(ret)return(ret);

    while(vf->offset<end){
      memset(og,0,sizeof(*og));
      ret=_get_next_page(vf,og,end-vf->offset);
      if(ret==OV_EREAD)return(OV_EREAD);
      if(ret<0){
        break;
      }else{
        offset=ret;
      }
    }
  }

  /* In a fully compliant, non-multiplexed stream, we'll still be
     holding the last page.  In multiplexed (or noncompliant streams),
     we will probably have to re-read the last page we saw */
  if(og->header_len==0){
    ret=_seek_helper(vf,offset);
    if(ret)return(ret);

    ret=_get_next_page(vf,og,CHUNKSIZE);
    if(ret<0)
      /* this shouldn't be possible */
      return(OV_EFAULT);
  }

  return(offset);
}

static void _add_serialno(ogg_page *og,long **serialno_list, int *n){
  long s = ogg_page_serialno(og);
  (*n)++;

  if(*serialno_list){
    *serialno_list = (long*)_ogg_realloc(*serialno_list, sizeof(**serialno_list)*(*n));
  }else{
    *serialno_list = (long*)_ogg_malloc(sizeof(**serialno_list));
  }

  (*serialno_list)[(*n)-1] = s;
}

/* returns nonzero if found */
static int _lookup_serialno(long s, long *serialno_list, int n){
  if(serialno_list){
    while(n--){
      if(*serialno_list == s) return 1;
      serialno_list++;
    }
  }
  return 0;
}

static int _lookup_page_serialno(ogg_page *og, long *serialno_list, int n){
  long s = ogg_page_serialno(og);
  return _lookup_serialno(s,serialno_list,n);
}

/* performs the same search as _get_prev_page, but prefers pages of
   the specified serial number. If a page of the specified serialno is
   spotted during the seek-back-and-read-forward, it will return the
   info of last page of the matching serial number instead of the very
   last page.  If no page of the specified serialno is seen, it will
   return the info of last page and alter *serialno.  */
static ogg_int64_t _get_prev_page_serial(OggVorbis_File *vf, ogg_int64_t begin,
                                         long *serial_list, int serial_n,
                                         int *serialno, ogg_int64_t *granpos){
  ogg_page og;
  ogg_int64_t end=begin;
  ogg_int64_t ret;

  ogg_int64_t prefoffset=-1;
  ogg_int64_t offset=-1;
  ogg_int64_t ret_serialno=-1;
  ogg_int64_t ret_gran=-1;

  while(offset==-1){
    begin-=CHUNKSIZE;
    if(begin<0)
      begin=0;

    ret=_seek_helper(vf,begin);
    if(ret)return(ret);

    while(vf->offset<end){
      ret=_get_next_page(vf,&og,end-vf->offset);
      if(ret==OV_EREAD)return(OV_EREAD);
      if(ret<0){
        break;
      }else{
        ret_serialno=ogg_page_serialno(&og);
        ret_gran=ogg_page_granulepos(&og);
        offset=ret;

        if(ret_serialno == *serialno){
          prefoffset=ret;
          *granpos=ret_gran;
        }

        if(!_lookup_serialno(ret_serialno,serial_list,serial_n)){
          /* we fell off the end of the link, which means we seeked
             back too far and shouldn't have been looking in that link
             to begin with.  If we found the preferred serial number,
             forget that we saw it. */
          prefoffset=-1;
        }
      }
    }
    /*We started from the beginning of the stream and found nothing.
      This should be impossible unless the contents of the stream changed out
      from under us after we read from it.*/
    if(!begin&&vf->offset<0)return OV_EBADLINK;
  }

  /* we're not interested in the page... just the serialno and granpos. */
  if(prefoffset>=0)return(prefoffset);

  *serialno = ret_serialno;
  *granpos = ret_gran;
  return(offset);

}

/* uses the local ogg_stream storage in vf; this is important for
   non-streaming input sources */
static int _fetch_headers(OggVorbis_File *vf,vorbis_info *vi,vorbis_comment *vc,
                          long **serialno_list, int *serialno_n,
                          ogg_page *og_ptr){
  ogg_page og;
  ogg_packet op;
  int i,ret;
  int allbos=0;

  if(!og_ptr){
    ogg_int64_t llret=_get_next_page(vf,&og,CHUNKSIZE);
    if(llret==OV_EREAD)return(OV_EREAD);
    if(llret<0)return(OV_ENOTVORBIS);
    og_ptr=&og;
  }

  vorbis_info_init(vi);
  vorbis_comment_init(vc);
  vf->ready_state=OPENED;

  /* extract the serialnos of all BOS pages + the first set of vorbis
     headers we see in the link */

  while(ogg_page_bos(og_ptr)){
    if(serialno_list){
      if(_lookup_page_serialno(og_ptr,*serialno_list,*serialno_n)){
        /* a dupe serialnumber in an initial header packet set == invalid stream */
        if(*serialno_list)_ogg_free(*serialno_list);
        *serialno_list=0;
        *serialno_n=0;
        ret=OV_EBADHEADER;
        goto bail_header;
      }

      _add_serialno(og_ptr,serialno_list,serialno_n);
    }

    if(vf->ready_state<STREAMSET){
      /* we don't have a vorbis stream in this link yet, so begin
         prospective stream setup. We need a stream to get packets */
      ogg_stream_reset_serialno(&vf->os,ogg_page_serialno(og_ptr));
      ogg_stream_pagein(&vf->os,og_ptr);

      if(ogg_stream_packetout(&vf->os,&op) > 0 &&
         vorbis_synthesis_idheader(&op)){
        /* vorbis header; continue setup */
        vf->ready_state=STREAMSET;
        if((ret=vorbis_synthesis_headerin(vi,vc,&op))){
          ret=OV_EBADHEADER;
          goto bail_header;
        }
      }
    }

    /* get next page */
    {
      ogg_int64_t llret=_get_next_page(vf,og_ptr,CHUNKSIZE);
      if(llret==OV_EREAD){
        ret=OV_EREAD;
        goto bail_header;
      }
      if(llret<0){
        ret=OV_ENOTVORBIS;
        goto bail_header;
      }

      /* if this page also belongs to our vorbis stream, submit it and break */
      if(vf->ready_state==STREAMSET &&
         vf->os.serialno == ogg_page_serialno(og_ptr)){
        ogg_stream_pagein(&vf->os,og_ptr);
        break;
      }
    }
  }

  if(vf->ready_state!=STREAMSET){
    ret = OV_ENOTVORBIS;
    goto bail_header;
  }

  while(1){

    i=0;
    while(i<2){ /* get a page loop */

      while(i<2){ /* get a packet loop */

        int result=ogg_stream_packetout(&vf->os,&op);
        if(result==0)break;
        if(result==-1){
          ret=OV_EBADHEADER;
          goto bail_header;
        }

        if((ret=vorbis_synthesis_headerin(vi,vc,&op)))
          goto bail_header;

        i++;
      }

      while(i<2){
        if(_get_next_page(vf,og_ptr,CHUNKSIZE)<0){
          ret=OV_EBADHEADER;
          goto bail_header;
        }

        /* if this page belongs to the correct stream, go parse it */
        if(vf->os.serialno == ogg_page_serialno(og_ptr)){
          ogg_stream_pagein(&vf->os,og_ptr);
          break;
        }

        /* if we never see the final vorbis headers before the link
           ends, abort */
        if(ogg_page_bos(og_ptr)){
          if(allbos){
            ret = OV_EBADHEADER;
            goto bail_header;
          }else
            allbos=1;
        }

        /* otherwise, keep looking */
      }
    }

    return 0;
  }

 bail_header:
  vorbis_info_clear(vi);
  vorbis_comment_clear(vc);
  vf->ready_state=OPENED;

  return ret;
}

/* Starting from current cursor position, get initial PCM offset of
   next page.  Consumes the page in the process without decoding
   audio, however this is only called during stream parsing upon
   seekable open. */
static ogg_int64_t _initial_pcmoffset(OggVorbis_File *vf, vorbis_info *vi){
  ogg_page    og;
  ogg_int64_t accumulated=0;
  long        lastblock=-1;
  int         result;
  int         serialno = vf->os.serialno;

  while(1){
    ogg_packet op;
    if(_get_next_page(vf,&og,-1)<0)
      break; /* should not be possible unless the file is truncated/mangled */

    if(ogg_page_bos(&og)) break;
    if(ogg_page_serialno(&og)!=serialno) continue;

    /* count blocksizes of all frames in the page */
    ogg_stream_pagein(&vf->os,&og);
    while((result=ogg_stream_packetout(&vf->os,&op))){
      if(result>0){ /* ignore holes */
        long thisblock=vorbis_packet_blocksize(vi,&op);
        if(thisblock>=0){
          if(lastblock!=-1)
            accumulated+=(lastblock+thisblock)>>2;
          lastblock=thisblock;
        }
      }
    }

    if(ogg_page_granulepos(&og)!=-1){
      /* pcm offset of last packet on the first audio page */
      accumulated= ogg_page_granulepos(&og)-accumulated;
      break;
    }
  }

  /* less than zero?  Either a corrupt file or a stream with samples
     trimmed off the beginning, a normal occurrence; in both cases set
     the offset to zero */
  if(accumulated<0)accumulated=0;

  return accumulated;
}

/* finds each bitstream link one at a time using a bisection search
   (has to begin by knowing the offset of the lb's initial page).
   Recurses for each link so it can alloc the link storage after
   finding them all, then unroll and fill the cache at the same time */
static int _bisect_forward_serialno(OggVorbis_File *vf,
                                    ogg_int64_t begin,
                                    ogg_int64_t searched,
                                    ogg_int64_t end,
                                    ogg_int64_t endgran,
                                    int endserial,
                                    long *currentno_list,
                                    int  currentnos,
                                    long m){
  ogg_int64_t pcmoffset;
  ogg_int64_t dataoffset=searched;
  ogg_int64_t endsearched=end;
  ogg_int64_t next=end;
  ogg_int64_t searchgran=-1;
  ogg_page og;
  ogg_int64_t ret,last;
  int serialno = vf->os.serialno;

  /* invariants:
     we have the headers and serialnos for the link beginning at 'begin'
     we have the offset and granpos of the last page in the file (potentially
       not a page we care about)
  */

  /* Is the last page in our list of current serialnumbers? */
  if(_lookup_serialno(endserial,currentno_list,currentnos)){

    /* last page is in the starting serialno list, so we've bisected
       down to (or just started with) a single link.  Now we need to
       find the last vorbis page belonging to the first vorbis stream
       for this link. */
    searched = end;
    while(endserial != serialno){
      endserial = serialno;
      searched=_get_prev_page_serial(vf,searched,currentno_list,currentnos,&endserial,&endgran);
    }

    vf->links=m+1;
    if(vf->offsets)_ogg_free(vf->offsets);
    if(vf->serialnos)_ogg_free(vf->serialnos);
    if(vf->dataoffsets)_ogg_free(vf->dataoffsets);

    vf->offsets=(ogg_int64_t*)_ogg_malloc((vf->links+1)*sizeof(*vf->offsets));
    vf->vi=(vorbis_info*)_ogg_realloc(vf->vi,vf->links*sizeof(*vf->vi));
    vf->vc=(vorbis_comment*)_ogg_realloc(vf->vc,vf->links*sizeof(*vf->vc));
    vf->serialnos=(long*)_ogg_malloc(vf->links*sizeof(*vf->serialnos));
    vf->dataoffsets=(ogg_int64_t*)_ogg_malloc(vf->links*sizeof(*vf->dataoffsets));
    vf->pcmlengths=(ogg_int64_t*)_ogg_malloc(vf->links*2*sizeof(*vf->pcmlengths));

    vf->offsets[m+1]=end;
    vf->offsets[m]=begin;
    vf->pcmlengths[m*2+1]=(endgran<0?0:endgran);

  }else{

    /* last page is not in the starting stream's serial number list,
       so we have multiple links.  Find where the stream that begins
       our bisection ends. */

    long *next_serialno_list=NULL;
    int next_serialnos=0;
    vorbis_info vi;
    vorbis_comment vc;
    int testserial = serialno+1;

    /* the below guards against garbage seperating the last and
       first pages of two links. */
    while(searched<endsearched){
      ogg_int64_t bisect;

      if(endsearched-searched<CHUNKSIZE){
        bisect=searched;
      }else{
        bisect=(searched+endsearched)/2;
      }

      ret=_seek_helper(vf,bisect);
      if(ret)return(ret);

      last=_get_next_page(vf,&og,-1);
      if(last==OV_EREAD)return(OV_EREAD);
      if(last<0 || !_lookup_page_serialno(&og,currentno_list,currentnos)){
        endsearched=bisect;
        if(last>=0)next=last;
      }else{
        searched=vf->offset;
      }
    }

    /* Bisection point found */
    /* for the time being, fetch end PCM offset the simple way */
    searched = next;
    while(testserial != serialno){
      testserial = serialno;
      searched = _get_prev_page_serial(vf,searched,currentno_list,currentnos,&testserial,&searchgran);
    }

    ret=_seek_helper(vf,next);
    if(ret)return(ret);

    ret=_fetch_headers(vf,&vi,&vc,&next_serialno_list,&next_serialnos,NULL);
    if(ret)return(ret);
    serialno = vf->os.serialno;
    dataoffset = vf->offset;

    /* this will consume a page, however the next bisection always
       starts with a raw seek */
    pcmoffset = _initial_pcmoffset(vf,&vi);

    ret=_bisect_forward_serialno(vf,next,vf->offset,end,endgran,endserial,
                                 next_serialno_list,next_serialnos,m+1);
    if(ret)return(ret);

    if(next_serialno_list)_ogg_free(next_serialno_list);

    vf->offsets[m+1]=next;
    vf->serialnos[m+1]=serialno;
    vf->dataoffsets[m+1]=dataoffset;

    vf->vi[m+1]=vi;
    vf->vc[m+1]=vc;

    vf->pcmlengths[m*2+1]=searchgran;
    vf->pcmlengths[m*2+2]=pcmoffset;
    vf->pcmlengths[m*2+3]-=pcmoffset;
    if(vf->pcmlengths[m*2+3]<0)vf->pcmlengths[m*2+3]=0;
  }
  return(0);
}

static int _make_decode_ready(OggVorbis_File *vf){
  if(vf->ready_state>STREAMSET)return 0;
  if(vf->ready_state<STREAMSET)return OV_EFAULT;
  if(vf->seekable){
    if(vorbis_synthesis_init(&vf->vd,vf->vi+vf->current_link))
      return OV_EBADLINK;
  }else{
    if(vorbis_synthesis_init(&vf->vd,vf->vi))
      return OV_EBADLINK;
  }
  vorbis_block_init(&vf->vd,&vf->vb);
  vf->ready_state=INITSET;
  vf->bittrack=0.f;
  vf->samptrack=0.f;
  return 0;
}

static int _open_seekable2(OggVorbis_File *vf){
  ogg_int64_t dataoffset=vf->dataoffsets[0],end,endgran=-1;
  int endserial=vf->os.serialno;
  int serialno=vf->os.serialno;

  /* we're partially open and have a first link header state in
     storage in vf */

  /* fetch initial PCM offset */
  ogg_int64_t pcmoffset = _initial_pcmoffset(vf,vf->vi);

  /* we can seek, so set out learning all about this file */
  if(vf->callbacks.seek_func && vf->callbacks.tell_func){
    (vf->callbacks.seek_func)(vf->datasource,0,SEEK_END);
    vf->offset=vf->end=(vf->callbacks.tell_func)(vf->datasource);
  }else{
    vf->offset=vf->end=-1;
  }

  /* If seek_func is implemented, tell_func must also be implemented */
  if(vf->end==-1) return(OV_EINVAL);

  /* Get the offset of the last page of the physical bitstream, or, if
     we're lucky the last vorbis page of this link as most OggVorbis
     files will contain a single logical bitstream */
  end=_get_prev_page_serial(vf,vf->end,vf->serialnos+2,vf->serialnos[1],&endserial,&endgran);
  if(end<0)return(end);

  /* now determine bitstream structure recursively */
  if(_bisect_forward_serialno(vf,0,dataoffset,end,endgran,endserial,
                              vf->serialnos+2,vf->serialnos[1],0)<0)return(OV_EREAD);

  vf->offsets[0]=0;
  vf->serialnos[0]=serialno;
  vf->dataoffsets[0]=dataoffset;
  vf->pcmlengths[0]=pcmoffset;
  vf->pcmlengths[1]-=pcmoffset;
  if(vf->pcmlengths[1]<0)vf->pcmlengths[1]=0;

  return(ov_raw_seek(vf,dataoffset));
}

/* clear out the current logical bitstream decoder */
static void _decode_clear(OggVorbis_File *vf){
  vorbis_dsp_clear(&vf->vd);
  vorbis_block_clear(&vf->vb);
  vf->ready_state=OPENED;
}

/* fetch and process a packet.  Handles the case where we're at a
   bitstream boundary and dumps the decoding machine.  If the decoding
   machine is unloaded, it loads it.  It also keeps pcm_offset up to
   date (seek and read both use this.  seek uses a special hack with
   readp).

   return: <0) error, OV_HOLE (lost packet) or OV_EOF
            0) need more data (only if readp==0)
            1) got a packet
*/

static int _fetch_and_process_packet(OggVorbis_File *vf,
                                     ogg_packet *op_in,
                                     int readp,
                                     int spanp){
  ogg_page og;

  /* handle one packet.  Try to fetch it from current stream state */
  /* extract packets from page */
  while(1){

    if(vf->ready_state==STREAMSET){
      int ret=_make_decode_ready(vf);
      if(ret<0)return ret;
    }

    /* process a packet if we can. */

    if(vf->ready_state==INITSET){
      int hs=vorbis_synthesis_halfrate_p(vf->vi);

      while(1) {
              ogg_packet op;
              ogg_packet *op_ptr=(op_in?op_in:&op);
        int result=ogg_stream_packetout(&vf->os,op_ptr);
        ogg_int64_t granulepos;

        op_in=NULL;
        if(result==-1)return(OV_HOLE); /* hole in the data. */
        if(result>0){
          /* got a packet.  process it */
          granulepos=op_ptr->granulepos;
          if(!vorbis_synthesis(&vf->vb,op_ptr)){ /* lazy check for lazy
                                                    header handling.  The
                                                    header packets aren't
                                                    audio, so if/when we
                                                    submit them,
                                                    vorbis_synthesis will
                                                    reject them */

            /* suck in the synthesis data and track bitrate */
            {
              int oldsamples=vorbis_synthesis_pcmout(&vf->vd,NULL);
              /* for proper use of libvorbis within libvorbisfile,
                 oldsamples will always be zero. */
              if(oldsamples)return(OV_EFAULT);

              vorbis_synthesis_blockin(&vf->vd,&vf->vb);
              vf->samptrack+=(vorbis_synthesis_pcmout(&vf->vd,NULL)<<hs);
              vf->bittrack+=op_ptr->bytes*8;
            }

            /* update the pcm offset. */
            if(granulepos!=-1 && !op_ptr->e_o_s){
              int link=(vf->seekable?vf->current_link:0);
              int i,samples;

              /* this packet has a pcm_offset on it (the last packet
                 completed on a page carries the offset) After processing
                 (above), we know the pcm position of the *last* sample
                 ready to be returned. Find the offset of the *first*

                 As an aside, this trick is inaccurate if we begin
                 reading anew right at the last page; the end-of-stream
                 granulepos declares the last frame in the stream, and the
                 last packet of the last page may be a partial frame.
                 So, we need a previous granulepos from an in-sequence page
                 to have a reference point.  Thus the !op_ptr->e_o_s clause
                 above */

              if(vf->seekable && link>0)
                granulepos-=vf->pcmlengths[link*2];
              if(granulepos<0)granulepos=0; /* actually, this
                                               shouldn't be possible
                                               here unless the stream
                                               is very broken */

              samples=(vorbis_synthesis_pcmout(&vf->vd,NULL)<<hs);

              granulepos-=samples;
              for(i=0;i<link;i++)
                granulepos+=vf->pcmlengths[i*2+1];
              vf->pcm_offset=granulepos;
            }
            return(1);
          }
        }
        else
          break;
      }
    }

    if(vf->ready_state>=OPENED){
      ogg_int64_t ret;

      while(1){
        /* the loop is not strictly necessary, but there's no sense in
           doing the extra checks of the larger loop for the common
           case in a multiplexed bistream where the page is simply
           part of a different logical bitstream; keep reading until
           we get one with the correct serialno */

        if(!readp)return(0);
        if((ret=_get_next_page(vf,&og,-1))<0){
          return(OV_EOF); /* eof. leave unitialized */
        }

        /* bitrate tracking; add the header's bytes here, the body bytes
           are done by packet above */
        vf->bittrack+=og.header_len*8;

        if(vf->ready_state==INITSET){
          if(vf->current_serialno!=ogg_page_serialno(&og)){

            /* two possibilities:
               1) our decoding just traversed a bitstream boundary
               2) another stream is multiplexed into this logical section */

            if(ogg_page_bos(&og)){
              /* boundary case */
              if(!spanp)
                return(OV_EOF);

              _decode_clear(vf);

              if(!vf->seekable){
                vorbis_info_clear(vf->vi);
                vorbis_comment_clear(vf->vc);
              }
              break;

            }else
              continue; /* possibility #2 */
          }
        }

        break;
      }
    }

    /* Do we need to load a new machine before submitting the page? */
    /* This is different in the seekable and non-seekable cases.

       In the seekable case, we already have all the header
       information loaded and cached; we just initialize the machine
       with it and continue on our merry way.

       In the non-seekable (streaming) case, we'll only be at a
       boundary if we just left the previous logical bitstream and
       we're now nominally at the header of the next bitstream
    */

    if(vf->ready_state!=INITSET){
      int link;

      if(vf->ready_state<STREAMSET){
        if(vf->seekable){
          long serialno = ogg_page_serialno(&og);

          /* match the serialno to bitstream section.  We use this rather than
             offset positions to avoid problems near logical bitstream
             boundaries */

          for(link=0;link<vf->links;link++)
            if(vf->serialnos[link]==serialno)break;

          if(link==vf->links) continue; /* not the desired Vorbis
                                           bitstream section; keep
                                           trying */

          vf->current_serialno=serialno;
          vf->current_link=link;

          ogg_stream_reset_serialno(&vf->os,vf->current_serialno);
          vf->ready_state=STREAMSET;

        }else{
          /* we're streaming */
          /* fetch the three header packets, build the info struct */

          int ret=_fetch_headers(vf,vf->vi,vf->vc,NULL,NULL,&og);
          if(ret)return(ret);
          vf->current_serialno=vf->os.serialno;
          vf->current_link++;
          link=0;
        }
      }
    }

    /* the buffered page is the data we want, and we're ready for it;
       add it to the stream state */
    ogg_stream_pagein(&vf->os,&og);

  }
}

/* if, eg, 64 bit stdio is configured by default, this will build with
   fseek64 */
static int _fseek64_wrap(FILE *f,ogg_int64_t off,int whence){
  if(f==NULL)return(-1);
  return fseek(f,off,whence);
}

static int _ov_open1(void *f,OggVorbis_File *vf,const char *initial,
                     long ibytes, ov_callbacks callbacks){
  int offsettest=((f && callbacks.seek_func)?callbacks.seek_func(f,0,SEEK_CUR):-1);
  long *serialno_list=NULL;
  int serialno_list_size=0;
  int ret;

  memset(vf,0,sizeof(*vf));
  vf->datasource=f;
  vf->callbacks = callbacks;

  /* init the framing state */
  ogg_sync_init(&vf->oy);

  /* perhaps some data was previously read into a buffer for testing
     against other stream types.  Allow initialization from this
     previously read data (especially as we may be reading from a
     non-seekable stream) */
  if(initial){
    char *buffer=ogg_sync_buffer(&vf->oy,ibytes);
    memcpy(buffer,initial,ibytes);
    ogg_sync_wrote(&vf->oy,ibytes);
  }

  /* can we seek? Stevens suggests the seek test was portable */
  if(offsettest!=-1)vf->seekable=1;

  /* No seeking yet; Set up a 'single' (current) logical bitstream
     entry for partial open */
  vf->links=1;
  vf->vi=(vorbis_info*)_ogg_calloc(vf->links,sizeof(*vf->vi));
  vf->vc=(vorbis_comment*)_ogg_calloc(vf->links,sizeof(*vf->vc));
  ogg_stream_init(&vf->os,-1); /* fill in the serialno later */

  /* Fetch all BOS pages, store the vorbis header and all seen serial
     numbers, load subsequent vorbis setup headers */
  if((ret=_fetch_headers(vf,vf->vi,vf->vc,&serialno_list,&serialno_list_size,NULL))<0){
    vf->datasource=NULL;
    ov_clear(vf);
  }else{
    /* serial number list for first link needs to be held somewhere
       for second stage of seekable stream open; this saves having to
       seek/reread first link's serialnumber data then. */
    vf->serialnos=(long*)_ogg_calloc(serialno_list_size+2,sizeof(*vf->serialnos));
    vf->serialnos[0]=vf->current_serialno=vf->os.serialno;
    vf->serialnos[1]=serialno_list_size;
    memcpy(vf->serialnos+2,serialno_list,serialno_list_size*sizeof(*vf->serialnos));

    vf->offsets=(ogg_int64_t*)_ogg_calloc(1,sizeof(*vf->offsets));
    vf->dataoffsets=(ogg_int64_t*)_ogg_calloc(1,sizeof(*vf->dataoffsets));
    vf->offsets[0]=0;
    vf->dataoffsets[0]=vf->offset;

    vf->ready_state=PARTOPEN;
  }
  if(serialno_list)_ogg_free(serialno_list);
  return(ret);
}

static int _ov_open2(OggVorbis_File *vf){
  if(vf->ready_state != PARTOPEN) return OV_EINVAL;
  vf->ready_state=OPENED;
  if(vf->seekable){
    int ret=_open_seekable2(vf);
    if(ret){
      vf->datasource=NULL;
      ov_clear(vf);
    }
    return(ret);
  }else
    vf->ready_state=STREAMSET;

  return 0;
}


/* clear out the OggVorbis_File struct */
int ov_clear(OggVorbis_File *vf){
  if(vf){
    vorbis_block_clear(&vf->vb);
    vorbis_dsp_clear(&vf->vd);
    ogg_stream_clear(&vf->os);

    if(vf->vi && vf->links){
      int i;
      for(i=0;i<vf->links;i++){
        vorbis_info_clear(vf->vi+i);
        vorbis_comment_clear(vf->vc+i);
      }
      _ogg_free(vf->vi);
      _ogg_free(vf->vc);
    }
    if(vf->dataoffsets)_ogg_free(vf->dataoffsets);
    if(vf->pcmlengths)_ogg_free(vf->pcmlengths);
    if(vf->serialnos)_ogg_free(vf->serialnos);
    if(vf->offsets)_ogg_free(vf->offsets);
    ogg_sync_clear(&vf->oy);
    if(vf->datasource && vf->callbacks.close_func)
      (vf->callbacks.close_func)(vf->datasource);
    memset(vf,0,sizeof(*vf));
  }
#ifdef DEBUG_LEAKS
  _VDBG_dump();
#endif
  return(0);
}

/* inspects the OggVorbis file and finds/documents all the logical
   bitstreams contained in it.  Tries to be tolerant of logical
   bitstream sections that are truncated/woogie.

   return: -1) error
            0) OK
*/

int ov_open_callbacks(void *f,OggVorbis_File *vf,
    const char *initial,long ibytes,ov_callbacks callbacks){
  int ret=_ov_open1(f,vf,initial,ibytes,callbacks);
  if(ret)return ret;
  return _ov_open2(vf);
}

int ov_open(FILE *f,OggVorbis_File *vf,const char *initial,long ibytes){
  ov_callbacks callbacks = {
    (size_t (*)(void *, size_t, size_t, void *))  fread,
    (int (*)(void *, ogg_int64_t, int))              _fseek64_wrap,
    (int (*)(void *))                             fclose,
    (long (*)(void *))                            ftell
  };

  return ov_open_callbacks((void *)f, vf, initial, ibytes, callbacks);
}

int ov_fopen(const char *path,OggVorbis_File *vf){
  int ret;
  FILE *f = fopen(path,"rb");
  if(!f) return -1;

  ret = ov_open(f,vf,NULL,0);
  if(ret) fclose(f);
  return ret;
}


/* cheap hack for game usage where downsampling is desirable; there's
   no need for SRC as we can just do it cheaply in libvorbis. */

int ov_halfrate(OggVorbis_File *vf,int flag){
  int i;
  if(vf->vi==NULL)return OV_EINVAL;
  if(vf->ready_state>STREAMSET){
    /* clear out stream state; dumping the decode machine is needed to
       reinit the MDCT lookups. */
    vorbis_dsp_clear(&vf->vd);
    vorbis_block_clear(&vf->vb);
    vf->ready_state=STREAMSET;
    if(vf->pcm_offset>=0){
      ogg_int64_t pos=vf->pcm_offset;
      vf->pcm_offset=-1; /* make sure the pos is dumped if unseekable */
      ov_pcm_seek(vf,pos);
    }
  }

  for(i=0;i<vf->links;i++){
    if(vorbis_synthesis_halfrate(vf->vi+i,flag)){
      if(flag) ov_halfrate(vf,0);
      return OV_EINVAL;
    }
  }
  return 0;
}

int ov_halfrate_p(OggVorbis_File *vf){
  if(vf->vi==NULL)return OV_EINVAL;
  return vorbis_synthesis_halfrate_p(vf->vi);
}

/* Only partially open the vorbis file; test for Vorbisness, and load
   the headers for the first chain.  Do not seek (although test for
   seekability).  Use ov_test_open to finish opening the file, else
   ov_clear to close/free it. Same return codes as open.

   Note that vorbisfile does _not_ take ownership of the file if the
   call fails; the calling applicaiton is responsible for closing the file
   if this call returns an error. */

int ov_test_callbacks(void *f,OggVorbis_File *vf,
    const char *initial,long ibytes,ov_callbacks callbacks)
{
  return _ov_open1(f,vf,initial,ibytes,callbacks);
}

int ov_test(FILE *f,OggVorbis_File *vf,const char *initial,long ibytes){
  ov_callbacks callbacks = {
    (size_t (*)(void *, size_t, size_t, void *))  fread,
    (int (*)(void *, ogg_int64_t, int))              _fseek64_wrap,
    (int (*)(void *))                             fclose,
    (long (*)(void *))                            ftell
  };

  return ov_test_callbacks((void *)f, vf, initial, ibytes, callbacks);
}

int ov_test_open(OggVorbis_File *vf){
  if(vf->ready_state!=PARTOPEN)return(OV_EINVAL);
  return _ov_open2(vf);
}

/* How many logical bitstreams in this physical bitstream? */
long ov_streams(OggVorbis_File *vf){
  return vf->links;
}

/* Is the FILE * associated with vf seekable? */
long ov_seekable(OggVorbis_File *vf){
  return vf->seekable;
}

/* returns the bitrate for a given logical bitstream or the entire
   physical bitstream.  If the file is open for random access, it will
   find the *actual* average bitrate.  If the file is streaming, it
   returns the nominal bitrate (if set) else the average of the
   upper/lower bounds (if set) else -1 (unset).

   If you want the actual bitrate field settings, get them from the
   vorbis_info structs */

long ov_bitrate(OggVorbis_File *vf,int i){
  if(vf->ready_state<OPENED)return(OV_EINVAL);
  if(i>=vf->links)return(OV_EINVAL);
  if(!vf->seekable && i!=0)return(ov_bitrate(vf,0));
  if(i<0){
    ogg_int64_t bits=0;
    int i;
    float br;
    for(i=0;i<vf->links;i++)
      bits+=(vf->offsets[i+1]-vf->dataoffsets[i])*8;
    /* This once read: return(rint(bits/ov_time_total(vf,-1)));
     * gcc 3.x on x86 miscompiled this at optimisation level 2 and above,
     * so this is slightly transformed to make it work.
     */
    br = bits/ov_time_total(vf,-1);
    return(rint(br));
  }else{
    if(vf->seekable){
      /* return the actual bitrate */
      return(rint((vf->offsets[i+1]-vf->dataoffsets[i])*8/ov_time_total(vf,i)));
    }else{
      /* return nominal if set */
      if(vf->vi[i].bitrate_nominal>0){
        return vf->vi[i].bitrate_nominal;
      }else{
        if(vf->vi[i].bitrate_upper>0){
          if(vf->vi[i].bitrate_lower>0){
            return (vf->vi[i].bitrate_upper+vf->vi[i].bitrate_lower)/2;
          }else{
            return vf->vi[i].bitrate_upper;
          }
        }
        return(OV_FALSE);
      }
    }
  }
}

/* returns the actual bitrate since last call.  returns -1 if no
   additional data to offer since last call (or at beginning of stream),
   EINVAL if stream is only partially open
*/
long ov_bitrate_instant(OggVorbis_File *vf){
  int link=(vf->seekable?vf->current_link:0);
  long ret;
  if(vf->ready_state<OPENED)return(OV_EINVAL);
  if(vf->samptrack==0)return(OV_FALSE);
  ret=vf->bittrack/vf->samptrack*vf->vi[link].rate+.5;
  vf->bittrack=0.f;
  vf->samptrack=0.f;
  return(ret);
}

/* Guess */
long ov_serialnumber(OggVorbis_File *vf,int i){
  if(i>=vf->links)return(ov_serialnumber(vf,vf->links-1));
  if(!vf->seekable && i>=0)return(ov_serialnumber(vf,-1));
  if(i<0){
    return(vf->current_serialno);
  }else{
    return(vf->serialnos[i]);
  }
}

/* returns: total raw (compressed) length of content if i==-1
            raw (compressed) length of that logical bitstream for i==0 to n
            OV_EINVAL if the stream is not seekable (we can't know the length)
            or if stream is only partially open
*/
ogg_int64_t ov_raw_total(OggVorbis_File *vf,int i){
  if(vf->ready_state<OPENED)return(OV_EINVAL);
  if(!vf->seekable || i>=vf->links)return(OV_EINVAL);
  if(i<0){
    ogg_int64_t acc=0;
    int i;
    for(i=0;i<vf->links;i++)
      acc+=ov_raw_total(vf,i);
    return(acc);
  }else{
    return(vf->offsets[i+1]-vf->offsets[i]);
  }
}

/* returns: total PCM length (samples) of content if i==-1 PCM length
            (samples) of that logical bitstream for i==0 to n
            OV_EINVAL if the stream is not seekable (we can't know the
            length) or only partially open
*/
ogg_int64_t ov_pcm_total(OggVorbis_File *vf,int i){
  if(vf->ready_state<OPENED)return(OV_EINVAL);
  if(!vf->seekable || i>=vf->links)return(OV_EINVAL);
  if(i<0){
    ogg_int64_t acc=0;
    int i;
    for(i=0;i<vf->links;i++)
      acc+=ov_pcm_total(vf,i);
    return(acc);
  }else{
    return(vf->pcmlengths[i*2+1]);
  }
}

/* returns: total seconds of content if i==-1
            seconds in that logical bitstream for i==0 to n
            OV_EINVAL if the stream is not seekable (we can't know the
            length) or only partially open
*/
double ov_time_total(OggVorbis_File *vf,int i){
  if(vf->ready_state<OPENED)return(OV_EINVAL);
  if(!vf->seekable || i>=vf->links)return(OV_EINVAL);
  if(i<0){
    double acc=0;
    int i;
    for(i=0;i<vf->links;i++)
      acc+=ov_time_total(vf,i);
    return(acc);
  }else{
    return((double)(vf->pcmlengths[i*2+1])/vf->vi[i].rate);
  }
}

/* seek to an offset relative to the *compressed* data. This also
   scans packets to update the PCM cursor. It will cross a logical
   bitstream boundary, but only if it can't get any packets out of the
   tail of the bitstream we seek to (so no surprises).

   returns zero on success, nonzero on failure */

int ov_raw_seek(OggVorbis_File *vf,ogg_int64_t pos){
  ogg_stream_state work_os;

  if(vf->ready_state<OPENED)return(OV_EINVAL);
  if(!vf->seekable)
    return(OV_ENOSEEK); /* don't dump machine if we can't seek */

  if(pos<0 || pos>vf->end)return(OV_EINVAL);

  /* is the seek position outside our current link [if any]? */
  if(vf->ready_state>=STREAMSET){
    if(pos<vf->offsets[vf->current_link] || pos>=vf->offsets[vf->current_link+1])
      _decode_clear(vf); /* clear out stream state */
  }

  /* don't yet clear out decoding machine (if it's initialized), in
     the case we're in the same link.  Restart the decode lapping, and
     let _fetch_and_process_packet deal with a potential bitstream
     boundary */
  vf->pcm_offset=-1;
  ogg_stream_reset_serialno(&vf->os,
                            vf->current_serialno); /* must set serialno */
  vorbis_synthesis_restart(&vf->vd);

  if(_seek_helper(vf,pos)) {
    /* dump the machine so we're in a known state */
    vf->pcm_offset=-1;
    _decode_clear(vf);
    return OV_EBADLINK;
  }

  /* we need to make sure the pcm_offset is set, but we don't want to
     advance the raw cursor past good packets just to get to the first
     with a granulepos.  That's not equivalent behavior to beginning
     decoding as immediately after the seek position as possible.

     So, a hack.  We use two stream states; a local scratch state and
     the shared vf->os stream state.  We use the local state to
     scan, and the shared state as a buffer for later decode.

     Unfortuantely, on the last page we still advance to last packet
     because the granulepos on the last page is not necessarily on a
     packet boundary, and we need to make sure the granpos is
     correct.
  */

  {
    ogg_page og;
    ogg_packet op;
    int lastblock=0;
    int accblock=0;
    int thisblock=0;
    int lastflag=0;
    int firstflag=0;
    ogg_int64_t pagepos=-1;

    ogg_stream_init(&work_os,vf->current_serialno); /* get the memory ready */
    ogg_stream_reset(&work_os); /* eliminate the spurious OV_HOLE
                                   return from not necessarily
                                   starting from the beginning */

    while(1){
      if(vf->ready_state>=STREAMSET){
        /* snarf/scan a packet if we can */
        int result=ogg_stream_packetout(&work_os,&op);

        if(result>0){

          if(vf->vi[vf->current_link].codec_setup){
            thisblock=vorbis_packet_blocksize(vf->vi+vf->current_link,&op);
            if(thisblock<0){
              ogg_stream_packetout(&vf->os,NULL);
              thisblock=0;
            }else{

              /* We can't get a guaranteed correct pcm position out of the
                 last page in a stream because it might have a 'short'
                 granpos, which can only be detected in the presence of a
                 preceding page.  However, if the last page is also the first
                 page, the granpos rules of a first page take precedence.  Not
                 only that, but for first==last, the EOS page must be treated
                 as if its a normal first page for the stream to open/play. */
              if(lastflag && !firstflag)
                ogg_stream_packetout(&vf->os,NULL);
              else
                if(lastblock)accblock+=(lastblock+thisblock)>>2;
            }

            if(op.granulepos!=-1){
              int i,link=vf->current_link;
              ogg_int64_t granulepos=op.granulepos-vf->pcmlengths[link*2];
              if(granulepos<0)granulepos=0;

              for(i=0;i<link;i++)
                granulepos+=vf->pcmlengths[i*2+1];
              vf->pcm_offset=granulepos-accblock;
              if(vf->pcm_offset<0)vf->pcm_offset=0;
              break;
            }
            lastblock=thisblock;
            continue;
          }else
            ogg_stream_packetout(&vf->os,NULL);
        }
      }

      if(!lastblock){
        pagepos=_get_next_page(vf,&og,-1);
        if(pagepos<0){
          vf->pcm_offset=ov_pcm_total(vf,-1);
          break;
        }
      }else{
        /* huh?  Bogus stream with packets but no granulepos */
        vf->pcm_offset=-1;
        break;
      }

      /* has our decoding just traversed a bitstream boundary? */
      if(vf->ready_state>=STREAMSET){
        if(vf->current_serialno!=ogg_page_serialno(&og)){

          /* two possibilities:
             1) our decoding just traversed a bitstream boundary
             2) another stream is multiplexed into this logical section? */

          if(ogg_page_bos(&og)){
            /* we traversed */
            _decode_clear(vf); /* clear out stream state */
            ogg_stream_clear(&work_os);
          } /* else, do nothing; next loop will scoop another page */
        }
      }

      if(vf->ready_state<STREAMSET){
        int link;
        long serialno = ogg_page_serialno(&og);

        for(link=0;link<vf->links;link++)
          if(vf->serialnos[link]==serialno)break;

        if(link==vf->links) continue; /* not the desired Vorbis
                                         bitstream section; keep
                                         trying */
        vf->current_link=link;
        vf->current_serialno=serialno;
        ogg_stream_reset_serialno(&vf->os,serialno);
        ogg_stream_reset_serialno(&work_os,serialno);
        vf->ready_state=STREAMSET;
        firstflag=(pagepos<=vf->dataoffsets[link]);
      }

      ogg_stream_pagein(&vf->os,&og);
      ogg_stream_pagein(&work_os,&og);
      lastflag=ogg_page_eos(&og);

    }
  }

  ogg_stream_clear(&work_os);
  vf->bittrack=0.f;
  vf->samptrack=0.f;
  return(0);
}

/* Page granularity seek (faster than sample granularity because we
   don't do the last bit of decode to find a specific sample).

   Seek to the last [granule marked] page preceding the specified pos
   location, such that decoding past the returned point will quickly
   arrive at the requested position. */
int ov_pcm_seek_page(OggVorbis_File *vf,ogg_int64_t pos){
  int link=-1;
  ogg_int64_t result=0;
  ogg_int64_t total=ov_pcm_total(vf,-1);

  if(vf->ready_state<OPENED)return(OV_EINVAL);
  if(!vf->seekable)return(OV_ENOSEEK);

  if(pos<0 || pos>total)return(OV_EINVAL);

  /* which bitstream section does this pcm offset occur in? */
  for(link=vf->links-1;link>=0;link--){
    total-=vf->pcmlengths[link*2+1];
    if(pos>=total)break;
  }

  /* Search within the logical bitstream for the page with the highest
     pcm_pos preceding pos.  If we're looking for a position on the
     first page, bisection will halt without finding our position as
     it's before the first explicit granulepos fencepost. That case is
     handled separately below.

     There is a danger here; missing pages or incorrect frame number
     information in the bitstream could make our task impossible.
     Account for that (it would be an error condition) */

  /* new search algorithm originally by HB (Nicholas Vinen) */

  {
    ogg_int64_t end=vf->offsets[link+1];
    ogg_int64_t begin=vf->dataoffsets[link];
    ogg_int64_t begintime = vf->pcmlengths[link*2];
    ogg_int64_t endtime = vf->pcmlengths[link*2+1]+begintime;
    ogg_int64_t target=pos-total+begintime;
    ogg_int64_t best=-1;
    int         got_page=0;

    ogg_page og;

    /* if we have only one page, there will be no bisection.  Grab the page here */
    if(begin==end){
      result=_seek_helper(vf,begin);
      if(result) goto seek_error;

      result=_get_next_page(vf,&og,1);
      if(result<0) goto seek_error;

      got_page=1;
    }

    /* bisection loop */
    while(begin<end){
      ogg_int64_t bisect;

      if(end-begin<CHUNKSIZE){
        bisect=begin;
      }else{
        /* take a (pretty decent) guess. */
        bisect=begin +
          (ogg_int64_t)((double)(target-begintime)*(end-begin)/(endtime-begintime))
          - CHUNKSIZE;
        if(bisect<begin+CHUNKSIZE)
          bisect=begin;
      }

      result=_seek_helper(vf,bisect);
      if(result) goto seek_error;

      /* read loop within the bisection loop */
      while(begin<end){
        result=_get_next_page(vf,&og,end-vf->offset);
        if(result==OV_EREAD) goto seek_error;
        if(result<0){
          /* there is no next page! */
          if(bisect<=begin+1)
              /* No bisection left to perform.  We've either found the
                 best candidate already or failed. Exit loop. */
            end=begin;
          else{
            /* We tried to load a fraction of the last page; back up a
               bit and try to get the whole last page */
            if(bisect==0) goto seek_error;
            bisect-=CHUNKSIZE;

            /* don't repeat/loop on a read we've already performed */
            if(bisect<=begin)bisect=begin+1;

            /* seek and cntinue bisection */
            result=_seek_helper(vf,bisect);
            if(result) goto seek_error;
          }
        }else{
          ogg_int64_t granulepos;
          got_page=1;

          /* got a page. analyze it */
          /* only consider pages from primary vorbis stream */
          if(ogg_page_serialno(&og)!=vf->serialnos[link])
            continue;

          /* only consider pages with the granulepos set */
          granulepos=ogg_page_granulepos(&og);
          if(granulepos==-1)continue;

          if(granulepos<target){
            /* this page is a successful candidate! Set state */

            best=result;  /* raw offset of packet with granulepos */
            begin=vf->offset; /* raw offset of next page */
            begintime=granulepos;

            /* if we're before our target but within a short distance,
               don't bisect; read forward */
            if(target-begintime>44100)break;

            bisect=begin; /* *not* begin + 1 as above */
          }else{

            /* This is one of our pages, but the granpos is
               post-target; it is not a bisection return
               candidate. (The only way we'd use it is if it's the
               first page in the stream; we handle that case later
               outside the bisection) */
            if(bisect<=begin+1){
              /* No bisection left to perform.  We've either found the
                 best candidate already or failed. Exit loop. */
              end=begin;
            }else{
              if(end==vf->offset){
                /* bisection read to the end; use the known page
                   boundary (result) to update bisection, back up a
                   little bit, and try again */
                end=result;
                bisect-=CHUNKSIZE;
                if(bisect<=begin)bisect=begin+1;
                result=_seek_helper(vf,bisect);
                if(result) goto seek_error;
              }else{
                /* Normal bisection */
                end=bisect;
                endtime=granulepos;
                break;
              }
            }
          }
        }
      }
    }

    /* Out of bisection: did it 'fail?' */
    if(best == -1){

      /* Check the 'looking for data in first page' special case;
         bisection would 'fail' because our search target was before the
         first PCM granule position fencepost. */

      if(got_page &&
         begin == vf->dataoffsets[link] &&
         ogg_page_serialno(&og)==vf->serialnos[link]){

        /* Yes, this is the beginning-of-stream case. We already have
           our page, right at the beginning of PCM data.  Set state
           and return. */

        vf->pcm_offset=total;

        if(link!=vf->current_link){
          /* Different link; dump entire decode machine */
          _decode_clear(vf);

          vf->current_link=link;
          vf->current_serialno=vf->serialnos[link];
          vf->ready_state=STREAMSET;

        }else{
          vorbis_synthesis_restart(&vf->vd);
        }

        ogg_stream_reset_serialno(&vf->os,vf->current_serialno);
        ogg_stream_pagein(&vf->os,&og);

      }else
        goto seek_error;

    }else{

      /* Bisection found our page. seek to it, update pcm offset. Easier case than
         raw_seek, don't keep packets preceding granulepos. */

      ogg_page og;
      ogg_packet op;

      /* seek */
      result=_seek_helper(vf,best);
      vf->pcm_offset=-1;
      if(result) goto seek_error;
      result=_get_next_page(vf,&og,-1);
      if(result<0) goto seek_error;

      if(link!=vf->current_link){
        /* Different link; dump entire decode machine */
        _decode_clear(vf);

        vf->current_link=link;
        vf->current_serialno=vf->serialnos[link];
        vf->ready_state=STREAMSET;

      }else{
        vorbis_synthesis_restart(&vf->vd);
      }

      ogg_stream_reset_serialno(&vf->os,vf->current_serialno);
      ogg_stream_pagein(&vf->os,&og);

      /* pull out all but last packet; the one with granulepos */
      while(1){
        result=ogg_stream_packetpeek(&vf->os,&op);
        if(result==0){
          /* No packet returned; we exited the bisection with 'best'
             pointing to a page with a granule position, so the packet
             finishing this page ('best') originated on a preceding
             page. Keep fetching previous pages until we get one with
             a granulepos or without the 'continued' flag set.  Then
             just use raw_seek for simplicity. */
          /* Do not rewind past the beginning of link data; if we do,
             it's either a bug or a broken stream */
          result=best;
          while(result>vf->dataoffsets[link]){
            result=_get_prev_page(vf,result,&og);
            if(result<0) goto seek_error;
            if(ogg_page_serialno(&og)==vf->current_serialno &&
               (ogg_page_granulepos(&og)>-1 ||
                !ogg_page_continued(&og))){
              return ov_raw_seek(vf,result);
            }
          }
        }
        if(result<0){
          result = OV_EBADPACKET;
          goto seek_error;
        }
        if(op.granulepos!=-1){
          vf->pcm_offset=op.granulepos-vf->pcmlengths[vf->current_link*2];
          if(vf->pcm_offset<0)vf->pcm_offset=0;
          vf->pcm_offset+=total;
          break;
        }else
          result=ogg_stream_packetout(&vf->os,NULL);
      }
    }
  }

  /* verify result */
  if(vf->pcm_offset>pos || pos>ov_pcm_total(vf,-1)){
    result=OV_EFAULT;
    goto seek_error;
  }
  vf->bittrack=0.f;
  vf->samptrack=0.f;
  return(0);

 seek_error:
  /* dump machine so we're in a known state */
  vf->pcm_offset=-1;
  _decode_clear(vf);
  return (int)result;
}

/* seek to a sample offset relative to the decompressed pcm stream
   returns zero on success, nonzero on failure */

int ov_pcm_seek(OggVorbis_File *vf,ogg_int64_t pos){
  int thisblock,lastblock=0;
  int ret=ov_pcm_seek_page(vf,pos);
  if(ret<0)return(ret);
  if((ret=_make_decode_ready(vf)))return ret;

  /* discard leading packets we don't need for the lapping of the
     position we want; don't decode them */

  while(1){
    ogg_packet op;
    ogg_page og;

    int ret=ogg_stream_packetpeek(&vf->os,&op);
    if(ret>0){
      thisblock=vorbis_packet_blocksize(vf->vi+vf->current_link,&op);
      if(thisblock<0){
        ogg_stream_packetout(&vf->os,NULL);
        continue; /* non audio packet */
      }
      if(lastblock)vf->pcm_offset+=(lastblock+thisblock)>>2;

      if(vf->pcm_offset+((thisblock+
                          vorbis_info_blocksize(vf->vi,1))>>2)>=pos)break;

      /* remove the packet from packet queue and track its granulepos */
      ogg_stream_packetout(&vf->os,NULL);
      vorbis_synthesis_trackonly(&vf->vb,&op);  /* set up a vb with
                                                   only tracking, no
                                                   pcm_decode */
      vorbis_synthesis_blockin(&vf->vd,&vf->vb);

      /* end of logical stream case is hard, especially with exact
         length positioning. */

      if(op.granulepos>-1){
        int i;
        /* always believe the stream markers */
        vf->pcm_offset=op.granulepos-vf->pcmlengths[vf->current_link*2];
        if(vf->pcm_offset<0)vf->pcm_offset=0;
        for(i=0;i<vf->current_link;i++)
          vf->pcm_offset+=vf->pcmlengths[i*2+1];
      }

      lastblock=thisblock;

    }else{
      if(ret<0 && ret!=OV_HOLE)break;

      /* suck in a new page */
      if(_get_next_page(vf,&og,-1)<0)break;
      if(ogg_page_bos(&og))_decode_clear(vf);

      if(vf->ready_state<STREAMSET){
        long serialno=ogg_page_serialno(&og);
        int link;

        for(link=0;link<vf->links;link++)
          if(vf->serialnos[link]==serialno)break;
        if(link==vf->links) continue;
        vf->current_link=link;

        vf->ready_state=STREAMSET;
        vf->current_serialno=ogg_page_serialno(&og);
        ogg_stream_reset_serialno(&vf->os,serialno);
        ret=_make_decode_ready(vf);
        if(ret)return ret;
        lastblock=0;
      }

      ogg_stream_pagein(&vf->os,&og);
    }
  }

  vf->bittrack=0.f;
  vf->samptrack=0.f;
  /* discard samples until we reach the desired position. Crossing a
     logical bitstream boundary with abandon is OK. */
  {
    /* note that halfrate could be set differently in each link, but
       vorbisfile encoforces all links are set or unset */
    int hs=vorbis_synthesis_halfrate_p(vf->vi);
    while(vf->pcm_offset<((pos>>hs)<<hs)){
      ogg_int64_t target=(pos-vf->pcm_offset)>>hs;
      long samples=vorbis_synthesis_pcmout(&vf->vd,NULL);

      if(samples>target)samples=target;
      vorbis_synthesis_read(&vf->vd,samples);
      vf->pcm_offset+=samples<<hs;

      if(samples<target)
        if(_fetch_and_process_packet(vf,NULL,1,1)<=0)
          vf->pcm_offset=ov_pcm_total(vf,-1); /* eof */
    }
  }
  return 0;
}

/* seek to a playback time relative to the decompressed pcm stream
   returns zero on success, nonzero on failure */
int ov_time_seek(OggVorbis_File *vf,double seconds){
  /* translate time to PCM position and call ov_pcm_seek */

  int link=-1;
  ogg_int64_t pcm_total=0;
  double time_total=0.;

  if(vf->ready_state<OPENED)return(OV_EINVAL);
  if(!vf->seekable)return(OV_ENOSEEK);
  if(seconds<0)return(OV_EINVAL);

  /* which bitstream section does this time offset occur in? */
  for(link=0;link<vf->links;link++){
    double addsec = ov_time_total(vf,link);
    if(seconds<time_total+addsec)break;
    time_total+=addsec;
    pcm_total+=vf->pcmlengths[link*2+1];
  }

  if(link==vf->links)return(OV_EINVAL);

  /* enough information to convert time offset to pcm offset */
  {
    ogg_int64_t target=pcm_total+(seconds-time_total)*vf->vi[link].rate;
    return(ov_pcm_seek(vf,target));
  }
}

/* page-granularity version of ov_time_seek
   returns zero on success, nonzero on failure */
int ov_time_seek_page(OggVorbis_File *vf,double seconds){
  /* translate time to PCM position and call ov_pcm_seek */

  int link=-1;
  ogg_int64_t pcm_total=0;
  double time_total=0.;

  if(vf->ready_state<OPENED)return(OV_EINVAL);
  if(!vf->seekable)return(OV_ENOSEEK);
  if(seconds<0)return(OV_EINVAL);

  /* which bitstream section does this time offset occur in? */
  for(link=0;link<vf->links;link++){
    double addsec = ov_time_total(vf,link);
    if(seconds<time_total+addsec)break;
    time_total+=addsec;
    pcm_total+=vf->pcmlengths[link*2+1];
  }

  if(link==vf->links)return(OV_EINVAL);

  /* enough information to convert time offset to pcm offset */
  {
    ogg_int64_t target=pcm_total+(seconds-time_total)*vf->vi[link].rate;
    return(ov_pcm_seek_page(vf,target));
  }
}

/* tell the current stream offset cursor.  Note that seek followed by
   tell will likely not give the set offset due to caching */
ogg_int64_t ov_raw_tell(OggVorbis_File *vf){
  if(vf->ready_state<OPENED)return(OV_EINVAL);
  return(vf->offset);
}

/* return PCM offset (sample) of next PCM sample to be read */
ogg_int64_t ov_pcm_tell(OggVorbis_File *vf){
  if(vf->ready_state<OPENED)return(OV_EINVAL);
  return(vf->pcm_offset);
}

/* return time offset (seconds) of next PCM sample to be read */
double ov_time_tell(OggVorbis_File *vf){
  int link=0;
  ogg_int64_t pcm_total=0;
  double time_total=0.f;

  if(vf->ready_state<OPENED)return(OV_EINVAL);
  if(vf->seekable){
    pcm_total=ov_pcm_total(vf,-1);
    time_total=ov_time_total(vf,-1);

    /* which bitstream section does this time offset occur in? */
    for(link=vf->links-1;link>=0;link--){
      pcm_total-=vf->pcmlengths[link*2+1];
      time_total-=ov_time_total(vf,link);
      if(vf->pcm_offset>=pcm_total)break;
    }
  }

  return((double)time_total+(double)(vf->pcm_offset-pcm_total)/vf->vi[link].rate);
}

/*  link:   -1) return the vorbis_info struct for the bitstream section
                currently being decoded
           0-n) to request information for a specific bitstream section

    In the case of a non-seekable bitstream, any call returns the
    current bitstream.  NULL in the case that the machine is not
    initialized */

vorbis_info *ov_info(OggVorbis_File *vf,int link){
  if(vf->seekable){
    if(link<0)
      if(vf->ready_state>=STREAMSET)
        return vf->vi+vf->current_link;
      else
      return vf->vi;
    else
      if(link>=vf->links)
        return NULL;
      else
        return vf->vi+link;
  }else{
    return vf->vi;
  }
}

/* grr, strong typing, grr, no templates/inheritence, grr */
vorbis_comment *ov_comment(OggVorbis_File *vf,int link){
  if(vf->seekable){
    if(link<0)
      if(vf->ready_state>=STREAMSET)
        return vf->vc+vf->current_link;
      else
        return vf->vc;
    else
      if(link>=vf->links)
        return NULL;
      else
        return vf->vc+link;
  }else{
    return vf->vc;
  }
}

static int host_is_big_endian() {
  ogg_int32_t pattern = 0xfeedface; /* deadbeef */
  unsigned char *bytewise = (unsigned char *)&pattern;
  if (bytewise[0] == 0xfe) return 1;
  return 0;
}

/* up to this point, everything could more or less hide the multiple
   logical bitstream nature of chaining from the toplevel application
   if the toplevel application didn't particularly care.  However, at
   the point that we actually read audio back, the multiple-section
   nature must surface: Multiple bitstream sections do not necessarily
   have to have the same number of channels or sampling rate.

   ov_read returns the sequential logical bitstream number currently
   being decoded along with the PCM data in order that the toplevel
   application can take action on channel/sample rate changes.  This
   number will be incremented even for streamed (non-seekable) streams
   (for seekable streams, it represents the actual logical bitstream
   index within the physical bitstream.  Note that the accessor
   functions above are aware of this dichotomy).

   ov_read_filter is exactly the same as ov_read except that it processes
   the decoded audio data through a filter before packing it into the
   requested format. This gives greater accuracy than applying a filter
   after the audio has been converted into integral PCM.

   input values: buffer) a buffer to hold packed PCM data for return
                 length) the byte length requested to be placed into buffer
                 bigendianp) should the data be packed LSB first (0) or
                             MSB first (1)
                 word) word size for output.  currently 1 (byte) or
                       2 (16 bit short)

   return values: <0) error/hole in data (OV_HOLE), partial open (OV_EINVAL)
                   0) EOF
                   n) number of bytes of PCM actually returned.  The
                   below works on a packet-by-packet basis, so the
                   return length is not related to the 'length' passed
                   in, just guaranteed to fit.

            *section) set to the logical bitstream number */

long ov_read_filter(OggVorbis_File *vf,char *buffer,int length,
                    int bigendianp,int word,int sgned,int *bitstream,
                    void (*filter)(float **pcm,long channels,long samples,void *filter_param),void *filter_param){
  int i,j;
  int host_endian = host_is_big_endian();
  int hs;

  float **pcm;
  long samples;

  if(vf->ready_state<OPENED)return(OV_EINVAL);
  if(word<=0)return(OV_EINVAL);

  while(1){
    if(vf->ready_state==INITSET){
      samples=vorbis_synthesis_pcmout(&vf->vd,&pcm);
      if(samples)break;
    }

    /* suck in another packet */
    {
      int ret=_fetch_and_process_packet(vf,NULL,1,1);
      if(ret==OV_EOF)
        return(0);
      if(ret<=0)
        return(ret);
    }

  }

  if(samples>0){

    /* yay! proceed to pack data into the byte buffer */

    long channels=ov_info(vf,-1)->channels;
    long bytespersample=word * channels;
    vorbis_fpu_control fpu;

    if(channels<1||channels>255)return(OV_EINVAL);
    if(samples>length/bytespersample)samples=length/bytespersample;

    if(samples <= 0)
      return OV_EINVAL;

    /* Here. */
    if(filter)
      filter(pcm,channels,samples,filter_param);

    /* a tight loop to pack each size */
    {
      int val;
      if(word==1){
        int off=(sgned?0:128);
        vorbis_fpu_setround(&fpu);
        for(j=0;j<samples;j++)
          for(i=0;i<channels;i++){
            val=vorbis_ftoi(pcm[i][j]*128.f);
            if(val>127)val=127;
            else if(val<-128)val=-128;
            *buffer++=val+off;
          }
        vorbis_fpu_restore(fpu);
      }else{
        int off=(sgned?0:32768);

        if(host_endian==bigendianp){
          if(sgned){

            vorbis_fpu_setround(&fpu);
            for(i=0;i<channels;i++) { /* It's faster in this order */
              float *src=pcm[i];
              short *dest=((short *)buffer)+i;
              for(j=0;j<samples;j++) {
                val=vorbis_ftoi(src[j]*32768.f);
                if(val>32767)val=32767;
                else if(val<-32768)val=-32768;
                *dest=val;
                dest+=channels;
              }
            }
            vorbis_fpu_restore(fpu);

          }else{

            vorbis_fpu_setround(&fpu);
            for(i=0;i<channels;i++) {
              float *src=pcm[i];
              short *dest=((short *)buffer)+i;
              for(j=0;j<samples;j++) {
                val=vorbis_ftoi(src[j]*32768.f);
                if(val>32767)val=32767;
                else if(val<-32768)val=-32768;
                *dest=val+off;
                dest+=channels;
              }
            }
            vorbis_fpu_restore(fpu);

          }
        }else if(bigendianp){

          vorbis_fpu_setround(&fpu);
          for(j=0;j<samples;j++)
            for(i=0;i<channels;i++){
              val=vorbis_ftoi(pcm[i][j]*32768.f);
              if(val>32767)val=32767;
              else if(val<-32768)val=-32768;
              val+=off;
              *buffer++=(val>>8);
              *buffer++=(val&0xff);
            }
          vorbis_fpu_restore(fpu);

        }else{
          int val;
          vorbis_fpu_setround(&fpu);
          for(j=0;j<samples;j++)
            for(i=0;i<channels;i++){
              val=vorbis_ftoi(pcm[i][j]*32768.f);
              if(val>32767)val=32767;
              else if(val<-32768)val=-32768;
              val+=off;
              *buffer++=(val&0xff);
              *buffer++=(val>>8);
                  }
          vorbis_fpu_restore(fpu);

        }
      }
    }

    vorbis_synthesis_read(&vf->vd,samples);
    hs=vorbis_synthesis_halfrate_p(vf->vi);
    vf->pcm_offset+=(samples<<hs);
    if(bitstream)*bitstream=vf->current_link;
    return(samples*bytespersample);
  }else{
    return(samples);
  }
}

long ov_read(OggVorbis_File *vf,char *buffer,int length,
             int bigendianp,int word,int sgned,int *bitstream){
  return ov_read_filter(vf, buffer, length, bigendianp, word, sgned, bitstream, NULL, NULL);
}

/* input values: pcm_channels) a float vector per channel of output
                 length) the sample length being read by the app

   return values: <0) error/hole in data (OV_HOLE), partial open (OV_EINVAL)
                   0) EOF
                   n) number of samples of PCM actually returned.  The
                   below works on a packet-by-packet basis, so the
                   return length is not related to the 'length' passed
                   in, just guaranteed to fit.

            *section) set to the logical bitstream number */



long ov_read_float(OggVorbis_File *vf,float ***pcm_channels,int length,
                   int *bitstream){

  if(vf->ready_state<OPENED)return(OV_EINVAL);

  while(1){
    if(vf->ready_state==INITSET){
      float **pcm;
      long samples=vorbis_synthesis_pcmout(&vf->vd,&pcm);
      if(samples){
        int hs=vorbis_synthesis_halfrate_p(vf->vi);
        if(pcm_channels)*pcm_channels=pcm;
        if(samples>length)samples=length;
        vorbis_synthesis_read(&vf->vd,samples);
        vf->pcm_offset+=samples<<hs;
        if(bitstream)*bitstream=vf->current_link;
        return samples;

      }
    }

    /* suck in another packet */
    {
      int ret=_fetch_and_process_packet(vf,NULL,1,1);
      if(ret==OV_EOF)return(0);
      if(ret<=0)return(ret);
    }

  }
}

extern const float *vorbis_window(vorbis_dsp_state *v,int W);

static void _ov_splice(float **pcm,float **lappcm,
                       int n1, int n2,
                       int ch1, int ch2,
                       const float *w1, const float *w2){
  int i,j;
  const float *w=w1;
  int n=n1;

  if(n1>n2){
    n=n2;
    w=w2;
  }

  /* splice */
  for(j=0;j<ch1 && j<ch2;j++){
    float *s=lappcm[j];
    float *d=pcm[j];

    for(i=0;i<n;i++){
      float wd=w[i]*w[i];
      float ws=1.-wd;
      d[i]=d[i]*wd + s[i]*ws;
    }
  }
  /* window from zero */
  for(;j<ch2;j++){
    float *d=pcm[j];
    for(i=0;i<n;i++){
      float wd=w[i]*w[i];
      d[i]=d[i]*wd;
    }
  }

}

/* make sure vf is INITSET */
static int _ov_initset(OggVorbis_File *vf){
  while(1){
    if(vf->ready_state==INITSET)break;
    /* suck in another packet */
    {
      int ret=_fetch_and_process_packet(vf,NULL,1,0);
      if(ret<0 && ret!=OV_HOLE)return(ret);
    }
  }
  return 0;
}

/* make sure vf is INITSET and that we have a primed buffer; if
   we're crosslapping at a stream section boundary, this also makes
   sure we're sanity checking against the right stream information */
static int _ov_initprime(OggVorbis_File *vf){
  vorbis_dsp_state *vd=&vf->vd;
  while(1){
    if(vf->ready_state==INITSET)
      if(vorbis_synthesis_pcmout(vd,NULL))break;

    /* suck in another packet */
    {
      int ret=_fetch_and_process_packet(vf,NULL,1,0);
      if(ret<0 && ret!=OV_HOLE)return(ret);
    }
  }
  return 0;
}

/* grab enough data for lapping from vf; this may be in the form of
   unreturned, already-decoded pcm, remaining PCM we will need to
   decode, or synthetic postextrapolation from last packets. */
static void _ov_getlap(OggVorbis_File *vf,vorbis_info *vi,vorbis_dsp_state *vd,
                       float **lappcm,int lapsize){
  int lapcount=0,i;
  float **pcm;

  /* try first to decode the lapping data */
  while(lapcount<lapsize){
    int samples=vorbis_synthesis_pcmout(vd,&pcm);
    if(samples){
      if(samples>lapsize-lapcount)samples=lapsize-lapcount;
      for(i=0;i<vi->channels;i++)
        memcpy(lappcm[i]+lapcount,pcm[i],sizeof(**pcm)*samples);
      lapcount+=samples;
      vorbis_synthesis_read(vd,samples);
    }else{
    /* suck in another packet */
      int ret=_fetch_and_process_packet(vf,NULL,1,0); /* do *not* span */
      if(ret==OV_EOF)break;
    }
  }
  if(lapcount<lapsize){
    /* failed to get lapping data from normal decode; pry it from the
       postextrapolation buffering, or the second half of the MDCT
       from the last packet */
    int samples=vorbis_synthesis_lapout(&vf->vd,&pcm);
    if(samples==0){
      for(i=0;i<vi->channels;i++)
        memset(lappcm[i]+lapcount,0,sizeof(**pcm)*lapsize-lapcount);
      lapcount=lapsize;
    }else{
      if(samples>lapsize-lapcount)samples=lapsize-lapcount;
      for(i=0;i<vi->channels;i++)
        memcpy(lappcm[i]+lapcount,pcm[i],sizeof(**pcm)*samples);
      lapcount+=samples;
    }
  }
}

/* this sets up crosslapping of a sample by using trailing data from
   sample 1 and lapping it into the windowing buffer of sample 2 */
int ov_crosslap(OggVorbis_File *vf1, OggVorbis_File *vf2){
  vorbis_info *vi1,*vi2;
  float **lappcm;
  float **pcm;
  const float *w1,*w2;
  int n1,n2,i,ret,hs1,hs2;

  if(vf1==vf2)return(0); /* degenerate case */
  if(vf1->ready_state<OPENED)return(OV_EINVAL);
  if(vf2->ready_state<OPENED)return(OV_EINVAL);

  /* the relevant overlap buffers must be pre-checked and pre-primed
     before looking at settings in the event that priming would cross
     a bitstream boundary.  So, do it now */

  ret=_ov_initset(vf1);
  if(ret)return(ret);
  ret=_ov_initprime(vf2);
  if(ret)return(ret);

  vi1=ov_info(vf1,-1);
  vi2=ov_info(vf2,-1);
  hs1=ov_halfrate_p(vf1);
  hs2=ov_halfrate_p(vf2);

  lappcm=(float**)alloca(sizeof(*lappcm)*vi1->channels);
  n1=vorbis_info_blocksize(vi1,0)>>(1+hs1);
  n2=vorbis_info_blocksize(vi2,0)>>(1+hs2);
  w1=vorbis_window(&vf1->vd,0);
  w2=vorbis_window(&vf2->vd,0);

  for(i=0;i<vi1->channels;i++)
    lappcm[i]=(float*)alloca(sizeof(**lappcm)*n1);

  _ov_getlap(vf1,vi1,&vf1->vd,lappcm,n1);

  /* have a lapping buffer from vf1; now to splice it into the lapping
     buffer of vf2 */
  /* consolidate and expose the buffer. */
  vorbis_synthesis_lapout(&vf2->vd,&pcm);

#if 0
  _analysis_output_always("pcmL",0,pcm[0],n1*2,0,0,0);
  _analysis_output_always("pcmR",0,pcm[1],n1*2,0,0,0);
#endif

  /* splice */
  _ov_splice(pcm,lappcm,n1,n2,vi1->channels,vi2->channels,w1,w2);

  /* done */
  return(0);
}

static int _ov_64_seek_lap(OggVorbis_File *vf,ogg_int64_t pos,
                           int (*localseek)(OggVorbis_File *,ogg_int64_t)){
  vorbis_info *vi;
  float **lappcm;
  float **pcm;
  const float *w1,*w2;
  int n1,n2,ch1,ch2,hs;
  int i,ret;

  if(vf->ready_state<OPENED)return(OV_EINVAL);
  ret=_ov_initset(vf);
  if(ret)return(ret);
  vi=ov_info(vf,-1);
  hs=ov_halfrate_p(vf);

  ch1=vi->channels;
  n1=vorbis_info_blocksize(vi,0)>>(1+hs);
  w1=vorbis_window(&vf->vd,0);  /* window arrays from libvorbis are
                                   persistent; even if the decode state
                                   from this link gets dumped, this
                                   window array continues to exist */

  lappcm=(float**)alloca(sizeof(*lappcm)*ch1);
  for(i=0;i<ch1;i++)
    lappcm[i]=(float*)alloca(sizeof(**lappcm)*n1);
  _ov_getlap(vf,vi,&vf->vd,lappcm,n1);

  /* have lapping data; seek and prime the buffer */
  ret=localseek(vf,pos);
  if(ret)return ret;
  ret=_ov_initprime(vf);
  if(ret)return(ret);

 /* Guard against cross-link changes; they're perfectly legal */
  vi=ov_info(vf,-1);
  ch2=vi->channels;
  n2=vorbis_info_blocksize(vi,0)>>(1+hs);
  w2=vorbis_window(&vf->vd,0);

  /* consolidate and expose the buffer. */
  vorbis_synthesis_lapout(&vf->vd,&pcm);

  /* splice */
  _ov_splice(pcm,lappcm,n1,n2,ch1,ch2,w1,w2);

  /* done */
  return(0);
}

int ov_raw_seek_lap(OggVorbis_File *vf,ogg_int64_t pos){
  return _ov_64_seek_lap(vf,pos,ov_raw_seek);
}

int ov_pcm_seek_lap(OggVorbis_File *vf,ogg_int64_t pos){
  return _ov_64_seek_lap(vf,pos,ov_pcm_seek);
}

int ov_pcm_seek_page_lap(OggVorbis_File *vf,ogg_int64_t pos){
  return _ov_64_seek_lap(vf,pos,ov_pcm_seek_page);
}

static int _ov_d_seek_lap(OggVorbis_File *vf,double pos,
                           int (*localseek)(OggVorbis_File *,double)){
  vorbis_info *vi;
  float **lappcm;
  float **pcm;
  const float *w1,*w2;
  int n1,n2,ch1,ch2,hs;
  int i,ret;

  if(vf->ready_state<OPENED)return(OV_EINVAL);
  ret=_ov_initset(vf);
  if(ret)return(ret);
  vi=ov_info(vf,-1);
  hs=ov_halfrate_p(vf);

  ch1=vi->channels;
  n1=vorbis_info_blocksize(vi,0)>>(1+hs);
  w1=vorbis_window(&vf->vd,0);  /* window arrays from libvorbis are
                                   persistent; even if the decode state
                                   from this link gets dumped, this
                                   window array continues to exist */

  lappcm=(float**)alloca(sizeof(*lappcm)*ch1);
  for(i=0;i<ch1;i++)
    lappcm[i]=(float*)alloca(sizeof(**lappcm)*n1);
  _ov_getlap(vf,vi,&vf->vd,lappcm,n1);

  /* have lapping data; seek and prime the buffer */
  ret=localseek(vf,pos);
  if(ret)return ret;
  ret=_ov_initprime(vf);
  if(ret)return(ret);

 /* Guard against cross-link changes; they're perfectly legal */
  vi=ov_info(vf,-1);
  ch2=vi->channels;
  n2=vorbis_info_blocksize(vi,0)>>(1+hs);
  w2=vorbis_window(&vf->vd,0);

  /* consolidate and expose the buffer. */
  vorbis_synthesis_lapout(&vf->vd,&pcm);

  /* splice */
  _ov_splice(pcm,lappcm,n1,n2,ch1,ch2,w1,w2);

  /* done */
  return(0);
}

int ov_time_seek_lap(OggVorbis_File *vf,double pos){
  return _ov_d_seek_lap(vf,pos,ov_time_seek);
}

int ov_time_seek_page_lap(OggVorbis_File *vf,double pos){
  return _ov_d_seek_lap(vf,pos,ov_time_seek_page);
}
#ifdef __cplusplus
}
#endif
#endif /* VORBIS_IMPL */
/*
Copyright (c) 2002-2020 Xiph.org Foundation
Copyright (c) 2020 Eduardo Bart (https://github.com/edubart)

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

- Redistributions of source code must retain the above copyright
notice, this list of conditions and the following disclaimer.

- Redistributions in binary form must reproduce the above copyright
notice, this list of conditions and the following disclaimer in the
documentation and/or other materials provided with the distribution.

- Neither the name of the Xiph.org Foundation nor the names of its
contributors may be used to endorse or promote products derived from
this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE FOUNDATION
OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
