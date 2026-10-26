#ifndef __NMD_HUD_COMPONENT_H__
#define __NMD_HUD_COMPONENT_H__

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

typedef enum nmdHudComponentVisualPreset_e : uint8_t
{
	NMD_HUD_COMPONENT_HIDDEN = 0,
	NMD_HUD_COMPONENT_FADE_OUT,
	NMD_HUD_COMPONENT_ALWAYS_VISIBLE
} nmdHudComponentVisualPreset_t;

class NMDHudComponent
{
public:
	NMDHudComponent( void );
	~NMDHudComponent();

	virtual void hide( void ) = 0;

	void render( void );
private:
};

#endif