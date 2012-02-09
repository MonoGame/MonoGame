LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := mojoshader

LOCAL_SRC_FILES := \
  mojoshader.c \
  mojoshader_assembler.c \
  mojoshader_common.c \
  mojoshader_compiler.c \
  mojoshader_effects.c \
  mojoshader_lexer.c \
  mojoshader_opengl.c \
  mojoshader_preprocessor.c \

include $(BUILD_SHARED_LIBRARY)
