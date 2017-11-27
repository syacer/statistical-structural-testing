#pragma once
namespace sutnamespace
{
	struct DATA {
		  int  key;
		  int  value;
	}  ;
	  
	class bs
	{
	  public:
		#ifdef DEBUG
		int cnt1;
		#endif 
		bs();
		~bs();
		struct DATA data[1000];
		int binarySearch(int x, int* countarray);
	  private:
		int bscore(int x, int* countarray);
	};
}