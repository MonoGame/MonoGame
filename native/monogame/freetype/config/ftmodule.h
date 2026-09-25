/*
 * MonoGame-owned FreeType module registration list.
 *
 * This list must stay aligned with the curated FreeType source files compiled
 * into mgruntime. It intentionally does not include the full upstream default
 * module set.
 *
 * FreeType intentionally includes FT_CONFIG_MODULES_H twice with different
 * FT_USE_MODULE macro definitions while building the default module table.
 * This file therefore must not use traditional include guards.
 */

FT_USE_MODULE( FT_Driver_ClassRec, tt_driver_class )
FT_USE_MODULE( FT_Driver_ClassRec, cff_driver_class )
FT_USE_MODULE( FT_Module_Class, psaux_module_class )
FT_USE_MODULE( FT_Module_Class, psnames_module_class )
FT_USE_MODULE( FT_Module_Class, sfnt_module_class )
FT_USE_MODULE( FT_Renderer_Class, ft_smooth_renderer_class )
FT_USE_MODULE( FT_Module_Class, pshinter_module_class )
