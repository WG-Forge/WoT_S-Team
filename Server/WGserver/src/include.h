#pragma once
#include "../../WGLib/src/IOBase/i_server.h"
#include "../../WGLib/src/Packets/packets.h"

namespace web
{
	namespace io_server
	{
		typedef i_server* (*create)(byte s_b1, byte s_b2, byte s_b3, byte s_b4, ushort port);
		typedef bool (*destroy)(i_server* web);

		class web_server_dll_loader
		{
		public:
			create create_fn = nullptr;
			destroy destroy_fn = nullptr;

			bool load(const char* path_dll)
			{
				HMODULE h = LoadLibraryA(path_dll);
				if (h)
				{
					_h = h;
					_loaded = true;

					create create_fn = (create)GetProcAddress(_h, "create");
					destroy destroy_fn = (destroy)GetProcAddress(_h, "destroy");
					if (create_fn && destroy_fn)
					{
						this->create_fn = create_fn;
						this->destroy_fn = destroy_fn;
						return _inited = true;
					}
					return false;
				}
				return false;
			}

			void unload()
			{
				if (_h) FreeLibrary(_h);
				_h = nullptr;
				_loaded = false;
				_inited = false;
				create_fn = nullptr;
				destroy_fn = nullptr;
			}

			bool is_loaded() { return _loaded; }
			bool is_inited() { return _inited; }

			~web_server_dll_loader()
			{
				unload();
			}
		private:
			bool _loaded = false;
			bool _inited = false;
			HMODULE _h = nullptr;
		};
	}
}