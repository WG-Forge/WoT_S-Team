#include "Server/server.h"
#include <sstream> //std::stringstream
#include <iostream>

int main(int argc, char* argv[])
{
	printf("Server WOT:S v.1.0.0.1\n");
	if (argc < 2) return 1;

	int b0, b1, b2, b3, port;
	{
		std::string ip = argv[1];
		std::stringstream s(ip);
		char ch;
		s >> b0 >> ch >> b1 >> ch >> b2 >> ch >> b3 >> ch >> port;
		printf("Address to connect: %d.%d.%d.%d:%d\n", b0, b1, b2, b3, port);
	}
	const char* path = "web_server.dll";

	web::io_server::web_server_dll_loader ld;
	ld.load(path);
	web::io_server::i_server* server_cl = ld.create_fn(b0, b1, b2, b3, port);
	server::server server(*server_cl);
	server.run();

	while (true)
	{
		std::string str;
		std::getline(std::cin, str);
		if (str.size() > 0)
		{
			if (str == "status")
				server.status();
			else 
				printf("invalid command\n");
		}
	}

	std::cin.get();
	return 0;
}