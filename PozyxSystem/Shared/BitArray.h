// BitArray.h

#ifndef _BITARRAY_h
#define _BITARRAY_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "Arduino.h"
#else
	#include <cstdint>
	#include <cstring>
#endif

class BitArray {
private:
	uint8_t* data;
	int16_t size;
public:
	BitArray() {}

	template<typename T>
	BitArray(const T& source) 
	{
		size = sizeof(T);
		data = new uint8_t[size];
		memcpy(data, &source, size);
	}

	BitArray(const BitArray& that) 
	{
		size = that.GetSize();
		data = new uint8_t[size];
		memcpy(data, that.GetPointerToData(), size);
	}

	BitArray& operator&(const BitArray& that) 
	{
		size = that.GetSize();
		if (data != that.GetPointerToData()) {
			delete[] data;
			data = new uint8_t[size];
			memcpy(data, that.GetPointerToData(), size);
		}
		return *this;
	}

	~BitArray() 
	{
		delete[] data;
	}

	bool operator==(const BitArray& rhs) const 
	{
		if (size != rhs.GetSize()) 
		{
			return false;
		}
		else if (data != rhs.GetPointerToData()) 
		{
				return false;
		}
		else 
		{
			return true;
		}
	}

	bool operator!=(const BitArray& rhs) const 
	{
		return !(*this == rhs);
	}

	template<typename T>
	void GetValue(const int i, T& dest) const 
	{
		memcpy(&dest, (data + i), sizeof(T));
		return;
	}

	int16_t GetSize() const 
	{
		return size;
	}

	uint8_t* GetPointerToData() const 
	{
		return data;
	}
};

#endif

