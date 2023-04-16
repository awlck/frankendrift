/*

FrankenDrift.GlkRunner.AsyncGlk runner
======================================

Copyright (c) 2023 Dannii Willis
MIT licenced
https://github.com/awlck/frankendrift

*/

import {dotnet} from './dotnet.js'

export class FrankenDrift {
    init(data, options) {
        this.data = data
        this.options = options
    }

    async start() {
        const Glk = this.options.Glk
        let object_id = 1
        const ids_to_objects = {}
        const objects_to_ids = new Map()
        const results = []

        function register_object(obj) {
            if (!obj) {
                return 0
            }
            const obj_id = object_id++
            ids_to_objects[obj_id] = obj
            objects_to_ids.set(obj, obj_id)
            return obj_id
        }
        function unregister_object(obj) {
            const obj_id = objects_to_ids.get(obj)
            delete ids_to_objects[obj_id]
            objects_to_ids.delete(obj)
        }
        function write_event(ev) {
            results[0] = ev.get_field(0)
            results[1] = objects_to_ids.get(ev.get_field(1)) || 0
            results[2] = ev.get_field(2)
            results[3] = ev.get_field(3)
        }

        const header = String.fromCharCode(...this.data.subarray(0, 12))
        const filename = 'storyfile.' + (header.startsWith('FORM') && header.endsWith('IFRS') ? 'blorb' : 'taf')

        const runtime = await dotnet
            .withApplicationArguments(filename)
            .withConfig({
                assets: [
                    {
                        behavior: 'vfs',
                        buffer: this.data,
                        name: filename,
                    },
                ],
            })
            .create()

        runtime.setModuleImports('main.js', {
            get_val: val => results[val],
            glk_cancel_hyperlink_event: winId => Glk.glk_cancel_hyperlink_event(ids_to_objects[winId]),
            glk_cancel_line_event: winId => {
                const res = new Glk.RefStruct()
                Glk.glk_cancel_line_event(ids_to_objects[winId], res)
                write_event(res)
            },
            glk_exit: () => Glk.glk_exit(),
            glk_fileref_create_by_name: (usage, name, fmode, rock) => register_object(Glk.glk_fileref_create_by_name(usage, name, fmode, rock)),
            glk_fileref_create_by_prompt: async (usage, fmode, rock) => {
                const fref = await new Promise(resolve => {
                    this.resume = resolve
                    Glk.glk_fileref_create_by_prompt(usage, fmode, rock)
                    Glk.update()
                })
                return register_object(fref)
            },
            glk_fileref_create_temp: (usage, rock) => register_object(Glk.glk_fileref_create_temp(usage, rock)),
            glk_fileref_destroy: frefID => {
                const fref = ids_to_objects[frefID]
                unregister_object(fref)
                Glk.glk_fileref_destroy(fref)
            },
            glk_gestalt: (sel, val) => Glk.glk_gestalt(sel, val),
            glk_gestalt_ext: (sel, val, arr) => Glk.glk_gestalt(sel, val, arr._unsafe_create_view()),
            glk_image_draw: (winId, imgId, val1, val2) => Glk.glk_image_draw(ids_to_objects[winId], imgId, val1, val2),
            glk_image_get_info: imgId => {
                const width = new Glk.RefBox()
                const height = new Glk.RefBox()
                Glk.glk_image_get_info(imgId, width, height)
                results[0] = width.get_value()
                results[1] = height.get_value()
            },
            glk_put_buffer: val => Glk.glk_put_buffer(val._unsafe_create_view()),
            glk_put_buffer_stream: (strId, val) => Glk.glk_put_buffer_stream(ids_to_objects[strId], val._unsafe_create_view()),
            glk_put_buffer_uni: val => Glk.glk_put_buffer_uni(val._unsafe_create_view()),
            glk_request_char_event: winId => Glk.glk_request_char_event(ids_to_objects[winId]),
            glk_request_hyperlink_event: winId => Glk.glk_request_hyperlink_event(ids_to_objects[winId]),
            glk_request_line_event: (winId, buf, initlen) => Glk.glk_request_line_event(ids_to_objects[winId], buf._unsafe_create_view(), initlen),
            glk_request_line_event_uni: (winId, buf, initlen) => Glk.glk_request_line_event_uni(ids_to_objects[winId], buf._unsafe_create_view(), initlen),
            glk_request_timer_events: msecs => Glk.glk_request_timer_events(msecs),
            glk_select: async () => {
                const res = new Glk.RefStruct()
                await new Promise(resolve => {
                    this.resume = resolve
                    Glk.glk_select(res)
                    Glk.update()
                })
                write_event(res)
            },
            glk_set_hyperlink: linkval => Glk.glk_set_hyperlink(linkval),
            glk_set_style: style => Glk.glk_set_style(style),
            glk_set_window: winId => Glk.glk_set_window(ids_to_objects[winId]),
            glk_stream_open_file: (frefId, fmode, rock) => {
                const fref = ids_to_objects[frefId]
                const str = Glk.glk_stream_open_file(fref, fmode, rock)
                if (str) {
                    return register_object(str)
                }
                return 0
            },
            glk_stream_open_memory: (buf, mode, rock) => {
                const str = Glk.glk_stream_open_memory(buf._unsafe_create_view(), mode, rock)
                if (str) {
                    return register_object(str)
                }
                return 0
            },
            glk_stream_set_position: (stream, pos, seekMode) => Glk.glk_stream_set_position(ids_to_objects[stream], pos, seekMode),
            glk_stylehint_set: (wintype, styl, hint, val) => Glk.glk_stylehint_set(wintype, styl, hint, val),
            glk_tick: () => Glk.glk_tick(),
            glk_window_clear: winId => Glk.glk_window_clear(ids_to_objects[winId]),
            glk_window_close: winId => {
                const win = ids_to_objects[winId]
                unregister_object(win)
                unregister_object(Glk.glk_window_get_stream(win))
                const res = new Glk.RefStruct()
                Glk.glk_window_close(win, res)
                results[0] = res.get_field(0)
                results[1] = res.get_field(1)
            },
            glk_window_flow_break: winId => Glk.glk_window_flow_break(ids_to_objects[winId]),
            glk_window_get_size: winId => {
                const width = new Glk.RefBox()
                const height = new Glk.RefBox()
                Glk.glk_window_get_size(ids_to_objects[winId], width, height)
                results[0] = width.get_value()
                results[1] = height.get_value()
            },
            glk_window_get_stream: winId => objects_to_ids.get(Glk.glk_window_get_stream(ids_to_objects[winId])),
            glk_window_move_cursor: (winId, xpos, ypos) => Glk.glk_window_move_cursor(ids_to_objects[winId], xpos, ypos),
            glk_window_open: (split, method, size, wintype, rock) => {
                const win = Glk.glk_window_open(split ? ids_to_objects[split]: null, method, size, wintype, rock)
                if (win) {
                    register_object(Glk.glk_window_get_stream(win))
                    return register_object(win)
                }
                return 0
            },
            garglk_set_zcolors: (fg, bg) => Glk.garglk_set_zcolors(fg, bg),
            glkunix_fileref_get_name: fileref => ids_to_objects[fileref].filename,
        })

        await dotnet.run()
    }
}