#pragma once
namespace sutnamespace
{	  
	class select
	{
	  public:
		select();
		~select();
		float arr[20];
		int Select(int k, int n, int* countarray);
	  private:
		float selectcore(unsigned long k, unsigned long n, int* countarray);
	};
}