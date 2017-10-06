#pragma once
namespace sutnamespace
{
	class nsichneuSUT
	{
		public:
			nsichneuSUT();
			~nsichneuSUT();
			int nsichneuSUT::Nsichneu(int in1, int in2, int in3,
				int in4, int in5, int in6, int in7, int in8, int in9, int in10,
				int in11, int in12, int in13, int in14, int *countarray);
		private:
			volatile int	P1_is_marked = 3;
			volatile long	P1_marking_member_0[3];
			volatile int	P2_is_marked = 5;
			volatile long	P2_marking_member_0[5];
			volatile int	P3_is_marked = 0;
			volatile long	P3_marking_member_0[6];
			int nsichneu(int* countarray);
	};
}

