/**************************************************************************/
/*  audio_stream_synchronized.hpp                                         */
/**************************************************************************/
/*                         This file is part of:                          */
/*                             GODOT ENGINE                               */
/*                        https://godotengine.org                         */
/**************************************************************************/
/* Copyright (c) 2014-present Godot Engine contributors (see AUTHORS.md). */
/* Copyright (c) 2007-2014 Juan Linietsky, Ariel Manzur.                  */
/*                                                                        */
/* Permission is hereby granted, free of charge, to any person obtaining  */
/* a copy of this software and associated documentation files (the        */
/* "Software"), to deal in the Software without restriction, including    */
/* without limitation the rights to use, copy, modify, merge, publish,    */
/* distribute, sublicense, and/or sell copies of the Software, and to     */
/* permit persons to whom the Software is furnished to do so, subject to  */
/* the following conditions:                                              */
/*                                                                        */
/* The above copyright notice and this permission notice shall be         */
/* included in all copies or substantial portions of the Software.        */
/*                                                                        */
/* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,        */
/* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF     */
/* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. */
/* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY   */
/* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,   */
/* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE      */
/* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                 */
/**************************************************************************/

// THIS FILE IS GENERATED. EDITS WILL BE LOST.

#pragma once

#include <godot_cpp/classes/audio_stream.hpp>
#include <godot_cpp/classes/ref.hpp>

#include <godot_cpp/core/class_db.hpp>

#include <type_traits>

namespace godot {

class AudioStreamSynchronized : public AudioStream {
	GDEXTENSION_CLASS(AudioStreamSynchronized, AudioStream)

public:
	static const int MAX_STREAMS = 32;

	void set_stream_count(int32_t p_stream_count);
	int32_t get_stream_count() const;
	void set_sync_stream(int32_t p_stream_index, const Ref<AudioStream> &p_audio_stream);
	Ref<AudioStream> get_sync_stream(int32_t p_stream_index) const;
	void set_sync_stream_volume(int32_t p_stream_index, float p_volume_db);
	float get_sync_stream_volume(int32_t p_stream_index) const;

protected:
	template <typename T, typename B>
	static void register_virtuals() {
		AudioStream::register_virtuals<T, B>();
	}

public:
};

} // namespace godot

