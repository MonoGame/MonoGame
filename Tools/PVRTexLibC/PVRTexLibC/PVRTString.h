/******************************************************************************

 @File         PVRTString.h

 @Title        PVRTString

 @Version      

 @Copyright    Copyright (c) Imagination Technologies Limited. All Rights Reserved. Strictly Confidential.

 @Platform     ANSI compatible

 @Description  A string class that can be used as drop-in replacement for
               std::string on platforms/compilers that don't provide a full C++
               standard library.

******************************************************************************/
#ifndef _PVRTSTRING_H_
#define _PVRTSTRING_H_

#include <stdio.h>
#define _USING_PVRTSTRING_

/*!***************************************************************************
@Class CPVRTString
@Brief A string class
*****************************************************************************/
class CPVRTString
{

private:

	// Checking printf and scanf format strings
#if defined(_CC_GNU_) || defined(__GNUG__) || defined(__GNUC__)
#define FX_PRINTF(fmt,arg) __attribute__((format(printf,fmt,arg)))
#define FX_SCANF(fmt,arg)  __attribute__((format(scanf,fmt,arg)))
#else
#define FX_PRINTF(fmt,arg)
#define FX_SCANF(fmt,arg)
#endif

public:
	typedef	size_t	size_type;
	typedef	char value_type;
	typedef	char& reference;
	typedef	const char& const_reference;

	static const size_type npos;


	

	/*!***********************************************************************
	@Function			CPVRTString
	@Input				_Ptr	A string
	@Input				_Count	Length of _Ptr
	@Description		Constructor
	************************************************************************/
	CPVRTString(const char* _Ptr, size_t _Count = npos);

	/*!***********************************************************************
	@Function			CPVRTString
	@Input				_Right	A string
	@Input				_Roff	Offset into _Right
	@Input				_Count	Number of chars from _Right to assign to the new string
	@Description		Constructor
	************************************************************************/
	CPVRTString(const CPVRTString& _Right, size_t _Roff = 0, size_t _Count = npos);

	/*!***********************************************************************
	@Function			CPVRTString
	@Input				_Count	Length of new string
	@Input				_Ch		A char to fill it with
	@Description		Constructor
	*************************************************************************/
	CPVRTString(size_t _Count, const char _Ch);

	/*!***********************************************************************
	@Function			CPVRTString
	@Input				_Ch	A char
	@Description		Constructor
	*************************************************************************/
	CPVRTString(const char _Ch);

	/*!***********************************************************************
	@Function			CPVRTString
	@Description		Constructor
	************************************************************************/
	CPVRTString();

	/*!***********************************************************************
	@Function			~CPVRTString
	@Description		Destructor
	************************************************************************/
	virtual ~CPVRTString();

	/*!***********************************************************************
	@Function			append
	@Input				_Ptr	A string
	@Returns			Updated string
	@Description		Appends a string
	*************************************************************************/
	CPVRTString& append(const char* _Ptr);

	/*!***********************************************************************
	@Function			append
	@Input				_Ptr	A string
	@Input				_Count	String length
	@Returns			Updated string
	@Description		Appends a string of length _Count
	*************************************************************************/
	CPVRTString& append(const char* _Ptr, size_t _Count);

	/*!***********************************************************************
	@Function			append
	@Input				_Str	A string
	@Returns			Updated string
	@Description		Appends a string
	*************************************************************************/
	CPVRTString& append(const CPVRTString& _Str);

	/*!***********************************************************************
	@Function			append
	@Input				_Str	A string
	@Input				_Off	A position in string
	@Input				_Count	Number of letters to append
	@Returns			Updated string
	@Description		Appends _Count letters of _Str from _Off in _Str
	*************************************************************************/
	CPVRTString& append(const CPVRTString& _Str, size_t _Off, size_t _Count);

	/*!***********************************************************************
	@Function			append
	@Input				_Ch		A char
	@Input				_Count	Number of times to append _Ch
	@Returns			Updated string
	@Description		Appends _Ch _Count times
	*************************************************************************/
	CPVRTString& append(size_t _Count, const char _Ch);

	//template<class InputIterator> CPVRTString& append(InputIterator _First, InputIterator _Last);

	/*!***********************************************************************
	@Function			assign
	@Input				_Ptr A string
	@Returns			Updated string
	@Description		Assigns the string to the string _Ptr
	*************************************************************************/
	CPVRTString& assign(const char* _Ptr);

	/*!***********************************************************************
	@Function			assign
	@Input				_Ptr A string
	@Input				_Count Length of _Ptr
	@Returns			Updated string
	@Description		Assigns the string to the string _Ptr
	*************************************************************************/
	CPVRTString& assign(const char* _Ptr, size_t _Count);

	/*!***********************************************************************
	@Function			assign
	@Input				_Str A string
	@Returns			Updated string
	@Description		Assigns the string to the string _Str
	*************************************************************************/
	CPVRTString& assign(const CPVRTString& _Str);

	/*!***********************************************************************
	@Function			assign
	@Input				_Str A string
	@Input				_Off First char to start assignment from
	@Input				_Count Length of _Str
	@Returns			Updated string
	@Description		Assigns the string to _Count characters in string _Str starting at _Off
	*************************************************************************/
	CPVRTString& assign(const CPVRTString& _Str, size_t _Off, size_t _Count=npos);

	/*!***********************************************************************
	@Function			assign
	@Input				_Ch A string
	@Input				_Count Number of times to repeat _Ch
	@Returns			Updated string
	@Description		Assigns the string to _Count copies of _Ch
	*************************************************************************/
	CPVRTString& assign(size_t _Count, char _Ch);

	//template<class InputIterator> CPVRTString& assign(InputIterator _First, InputIterator _Last);

	//const_reference at(size_t _Off) const;
	//reference at(size_t _Off);

	// const_iterator begin() const;
	// iterator begin();

	/*!***********************************************************************
	@Function			c_str
	@Returns			const char* pointer of the string
	@Description		Returns a const char* pointer of the string
	*************************************************************************/
	const char* c_str() const;

	/*!***********************************************************************
	@Function			capacity
	@Returns			The size of the character array reserved
	@Description		Returns the size of the character array reserved
	*************************************************************************/
	size_t capacity() const;

	/*!***********************************************************************
	@Function			clear
	@Description		Clears the string
	*************************************************************************/
	void clear();

	/*!***********************************************************************
	@Function			compare
	@Input				_Str A string to compare with
	@Returns			0 if the strings match
	@Description		Compares the string with _Str
	*************************************************************************/
	int compare(const CPVRTString& _Str) const;

	/*!***********************************************************************
	@Function			compare
	@Input				_Pos1	Position to start comparing from
	@Input				_Num1	Number of chars to compare
	@Input				_Str 	A string to compare with
	@Returns			0 if the strings match
	@Description		Compares the string with _Str
	*************************************************************************/
	int compare(size_t _Pos1, size_t _Num1, const CPVRTString& _Str) const;

	/*!***********************************************************************
	@Function			compare
	@Input				_Pos1	Position to start comparing from
	@Input				_Num1	Number of chars to compare
	@Input				_Str 	A string to compare with
	@Input				_Off 	Position in _Str to compare from
	@Input				_Count	Number of chars in _Str to compare with
	@Returns			0 if the strings match
	@Description		Compares the string with _Str
	*************************************************************************/
	int compare(size_t _Pos1, size_t _Num1, const CPVRTString& _Str, size_t _Off, size_t _Count) const;

	/*!***********************************************************************
	@Function			compare
	@Input				_Ptr A string to compare with
	@Returns			0 if the strings match
	@Description		Compares the string with _Ptr
	*************************************************************************/
	int compare(const char* _Ptr) const;

	/*!***********************************************************************
	@Function			compare
	@Input				_Pos1	Position to start comparing from
	@Input				_Num1	Number of chars to compare
	@Input				_Ptr 	A string to compare with
	@Returns			0 if the strings match
	@Description		Compares the string with _Ptr
	*************************************************************************/
	int compare(size_t _Pos1, size_t _Num1, const char* _Ptr) const;

	/*!***********************************************************************
	@Function			compare
	@Input				_Pos1	Position to start comparing from
	@Input				_Num1	Number of chars to compare
	@Input				_Ptr 	A string to compare with
	@Input				_Count	Number of chars to compare
	@Returns			0 if the strings match
	@Description		Compares the string with _Str
	*************************************************************************/
	int compare(size_t _Pos1, size_t _Num1, const char* _Ptr, size_t _Count) const;

	/*!***********************************************************************
	@Function			<
	@Input				_Str A string to compare with
	@Returns			True on success
	@Description		Less than operator
	*************************************************************************/
	bool operator<(const CPVRTString & _Str) const;

	/*!***********************************************************************
	@Function		==
	@Input			_Str 	A string to compare with
	@Returns		True if they match
	@Description	== Operator
	*************************************************************************/
	bool operator==(const CPVRTString& _Str) const;

	/*!***********************************************************************
	@Function		==
	@Input			_Ptr 	A string to compare with
	@Returns		True if they match
	@Description	== Operator
	*************************************************************************/
	bool operator==(const char* const _Ptr) const;

	/*!***********************************************************************
	@Function			!=
	@Input				_Str 	A string to compare with
	@Returns			True if they don't match
	@Description		!= Operator
	*************************************************************************/
	bool operator!=(const CPVRTString& _Str) const;

	/*!***********************************************************************
	@Function			!=
	@Input				_Ptr 	A string to compare with
	@Returns			True if they don't match
	@Description		!= Operator
	*************************************************************************/
	bool operator!=(const char* const _Ptr) const;

	/*!***********************************************************************
	@Function			copy
	@Modified			_Ptr 	A string to copy to
	@Input				_Count	Size of _Ptr
	@Input				_Off	Position to start copying from
	@Returns			Number of bytes copied
	@Description		Copies the string to _Ptr
	*************************************************************************/
	size_t copy(char* _Ptr, size_t _Count, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			data
	@Returns			A const char* version of the string
	@Description		Returns a const char* version of the string
	*************************************************************************/
	const char* data( ) const;

	/*!***********************************************************************
	@Function			empty
	@Returns			True if the string is empty
	@Description		Returns true if the string is empty
	*************************************************************************/
	bool empty() const;

	// const_iterator end() const;
	// iterator end();

	//iterator erase(iterator _First, iterator _Last);
	//iterator erase(iterator _It);

	/*!***********************************************************************
	@Function			erase
	@Input				_Pos	The position to start erasing from
	@Input				_Count	Number of chars to erase
	@Returns			An updated string
	@Description		Erases a portion of the string
	*************************************************************************/
	CPVRTString& erase(size_t _Pos = 0, size_t _Count = npos);

	/*!***********************************************************************
	@Function			substitute
	@Input				_src	Character to search
	@Input				_subDes	Character to substitute for
	@Input				_all	Substitute all
	@Returns			An updated string
	@Description		Erases a portion of the string
	*************************************************************************/
	CPVRTString& substitute(char _src,char _subDes, bool _all = true);

	/*!***********************************************************************
	@Function			substitute
	@Input				_src	Character to search
	@Input				_subDes	Character to substitute for
	@Input				_all	Substitute all
	@Returns			An updated string
	@Description		Erases a portion of the string
	*************************************************************************/
	CPVRTString& substitute(const char* _src, const char* _subDes, bool _all = true);

	//size_t find(char _Ch, size_t _Off = 0) const;
	//size_t find(const char* _Ptr, size_t _Off = 0) const;
	//size_t find(const char* _Ptr, size_t _Off = 0, size_t _Count) const;
	//size_t find(const CPVRTString& _Str, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_first_not_of
	@Input				_Ch		A char
	@Input				_Off	Start position of the find
	@Returns			Position of the first char that is not _Ch
	@Description		Returns the position of the first char that is not _Ch
	*************************************************************************/
	size_t find_first_not_of(char _Ch, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_first_not_of
	@Input				_Ptr	A string
	@Input				_Off	Start position of the find
	@Returns			Position of the first char that is not in _Ptr
	@Description		Returns the position of the first char that is not in _Ptr
	*************************************************************************/
	size_t find_first_not_of(const char* _Ptr, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_first_not_of
	@Input				_Ptr	A string
	@Input				_Off	Start position of the find
	@Input				_Count	Number of chars in _Ptr
	@Returns			Position of the first char that is not in _Ptr
	@Description		Returns the position of the first char that is not in _Ptr
	*************************************************************************/
	size_t find_first_not_of(const char* _Ptr, size_t _Off, size_t _Count) const;

	/*!***********************************************************************
	@Function			find_first_not_of
	@Input				_Str	A string
	@Input				_Off	Start position of the find
	@Returns			Position of the first char that is not in _Str
	@Description		Returns the position of the first char that is not in _Str
	*************************************************************************/
	size_t find_first_not_of(const CPVRTString& _Str, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_first_of
	@Input				_Ch		A char
	@Input				_Off	Start position of the find
	@Returns			Position of the first char that is _Ch
	@Description		Returns the position of the first char that is _Ch
	*************************************************************************/
	size_t find_first_of(char _Ch, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_first_of
	@Input				_Ptr	A string
	@Input				_Off	Start position of the find
	@Returns			Position of the first char that matches a char in _Ptr
	@Description		Returns the position of the first char that matches a char in _Ptr
	*************************************************************************/
	size_t find_first_of(const char* _Ptr, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_first_of
	@Input				_Ptr	A string
	@Input				_Off	Start position of the find
	@Input				_Count	Size of _Ptr
	@Returns			Position of the first char that matches a char in _Ptr
	@Description		Returns the position of the first char that matches a char in _Ptr
	*************************************************************************/
	size_t find_first_of(const char* _Ptr, size_t _Off, size_t _Count) const;

	/*!***********************************************************************
	@Function			find_first_ofn
	@Input				_Ptr	A string
	@Input				_Off	Start position of the find
	@Input				_Count	Size of _Ptr
	@Returns			Position of the first char that matches a char in _Ptr
	@Description		Returns the position of the first char that matches all chars in _Ptr
	*************************************************************************/
	size_t find_first_ofn(const char* _Ptr, size_t _Off, size_t _Count) const;
	

	/*!***********************************************************************
	@Function			find_first_of
	@Input				_Str	A string
	@Input				_Off	Start position of the find
	@Returns			Position of the first char that matches a char in _Str
	@Description		Returns the position of the first char that matches a char in _Str
	*************************************************************************/
	size_t find_first_of(const CPVRTString& _Str, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_last_not_of
	@Input				_Ch		A char
	@Input				_Off	Start position of the find
	@Returns			Position of the last char that is not _Ch
	@Description		Returns the position of the last char that is not _Ch
	*************************************************************************/
	size_t find_last_not_of(char _Ch, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_last_not_of
	@Input				_Ptr	A string
	@Input				_Off	Start position of the find
	@Returns			Position of the last char that is not in _Ptr
	@Description		Returns the position of the last char that is not in _Ptr
	*************************************************************************/
	size_t find_last_not_of(const char* _Ptr, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_last_not_of
	@Input				_Ptr	A string
	@Input				_Off	Start position of the find
	@Input				_Count	Length of _Ptr
	@Returns			Position of the last char that is not in _Ptr
	@Description		Returns the position of the last char that is not in _Ptr
	*************************************************************************/
	size_t find_last_not_of(const char* _Ptr, size_t _Off, size_t _Count) const;

	/*!***********************************************************************
	@Function			find_last_not_of
	@Input				_Str	A string
	@Input				_Off	Start position of the find
	@Returns			Position of the last char that is not in _Str
	@Description		Returns the position of the last char that is not in _Str
	*************************************************************************/
	size_t find_last_not_of(const CPVRTString& _Str, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_last_of
	@Input				_Ch		A char
	@Input				_Off	Start position of the find
	@Returns			Position of the last char that is _Ch
	@Description		Returns the position of the last char that is _Ch
	*************************************************************************/
	size_t find_last_of(char _Ch, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_last_of
	@Input				_Ptr	A string
	@Input				_Off	Start position of the find
	@Returns			Position of the last char that is in _Ptr
	@Description		Returns the position of the last char that is in _Ptr
	*************************************************************************/
	size_t find_last_of(const char* _Ptr, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_last_of
	@Input				_Ptr	A string
	@Input				_Off	Start position of the find
	@Input				_Count	Length of _Ptr
	@Returns			Position of the last char that is in _Ptr
	@Description		Returns the position of the last char that is in _Ptr
	*************************************************************************/
	size_t find_last_of(const char* _Ptr, size_t _Off, size_t _Count) const;

	/*!***********************************************************************
	@Function			find_last_of
	@Input				_Str	A string
	@Input				_Off	Start position of the find
	@Returns			Position of the last char that is in _Str
	@Description		Returns the position of the last char that is in _Str
	*************************************************************************/
	size_t find_last_of(const CPVRTString& _Str, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_number_of
	@Input				_Ch		A char
	@Input				_Off	Start position of the find
	@Returns			Number of occurances of _Ch in the parent string.
	@Description		Returns the number of occurances of _Ch in the parent string.
	*************************************************************************/
	size_t find_number_of(char _Ch, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_number_of
	@Input				_Ptr	A string
	@Input				_Off	Start position of the find
	@Returns			Number of occurances of _Ptr in the parent string.
	@Description		Returns the number of occurances of _Ptr in the parent string.
	*************************************************************************/
	size_t find_number_of(const char* _Ptr, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_number_of
	@Input				_Ptr	A string
	@Input				_Off	Start position of the find
	@Input				_Count	Size of _Ptr
	@Returns			Number of occurances of _Ptr in the parent string.
	@Description		Returns the number of occurances of _Ptr in the parent string.
	*************************************************************************/
	size_t find_number_of(const char* _Ptr, size_t _Off, size_t _Count) const;

	/*!***********************************************************************
	@Function			find_number_of
	@Input				_Str	A string
	@Input				_Off	Start position of the find
	@Returns			Number of occurances of _Str in the parent string.
	@Description		Returns the number of occurances of _Str in the parent string.
	*************************************************************************/
	size_t find_number_of(const CPVRTString& _Str, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_next_occurance_of
	@Input				_Ch		A char
	@Input				_Off	Start position of the find
	@Returns			Next occurance of _Ch in the parent string.
	@Description		Returns the next occurance of _Ch in the parent string
	after or at _Off.	If not found, returns the length of the string.
	*************************************************************************/
	int find_next_occurance_of(char _Ch, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_next_occurance_of
	@Input				_Ptr	A string
	@Input				_Off	Start position of the find
	@Returns			Next occurance of _Ptr in the parent string.
	@Description		Returns the next occurance of _Ptr in the parent string
	after or at _Off.	If not found, returns the length of the string.
	*************************************************************************/
	int find_next_occurance_of(const char* _Ptr, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_next_occurance_of
	@Input				_Ptr	A string
	@Input				_Off	Start position of the find
	@Input				_Count	Size of _Ptr
	@Returns			Next occurance of _Ptr in the parent string.
	@Description		Returns the next occurance of _Ptr in the parent string
	after or at _Off.	If not found, returns the length of the string.
	*************************************************************************/
	int find_next_occurance_of(const char* _Ptr, size_t _Off, size_t _Count) const;

	/*!***********************************************************************
	@Function			find_next_occurance_of
	@Input				_Str	A string
	@Input				_Off	Start position of the find
	@Returns			Next occurance of _Str in the parent string.
	@Description		Returns the next occurance of _Str in the parent string
	after or at _Off.	If not found, returns the length of the string.
	*************************************************************************/
	int find_next_occurance_of(const CPVRTString& _Str, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_previous_occurance_of
	@Input				_Ch		A char
	@Input				_Off	Start position of the find
	@Returns			Previous occurance of _Ch in the parent string.
	@Description		Returns the previous occurance of _Ch in the parent string
	before _Off.	If not found, returns -1.
	*************************************************************************/
	int find_previous_occurance_of(char _Ch, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_previous_occurance_of
	@Input				_Ptr	A string
	@Input				_Off	Start position of the find
	@Returns			Previous occurance of _Ptr in the parent string.
	@Description		Returns the previous occurance of _Ptr in the parent string
	before _Off.	If not found, returns -1.
	*************************************************************************/
	int find_previous_occurance_of(const char* _Ptr, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			find_previous_occurance_of
	@Input				_Ptr	A string
	@Input				_Off	Start position of the find
	@Input				_Count	Size of _Ptr
	@Returns			Previous occurance of _Ptr in the parent string.
	@Description		Returns the previous occurance of _Ptr in the parent string
	before _Off.	If not found, returns -1.
	*************************************************************************/
	int find_previous_occurance_of(const char* _Ptr, size_t _Off, size_t _Count) const;

	/*!***********************************************************************
	@Function			find_previous_occurance_of
	@Input				_Str	A string
	@Input				_Off	Start position of the find
	@Returns			Previous occurance of _Str in the parent string.
	@Description		Returns the previous occurance of _Str in the parent string
	before _Off.	If not found, returns -1.
	*************************************************************************/
	int find_previous_occurance_of(const CPVRTString& _Str, size_t _Off = 0) const;

	/*!***********************************************************************
	@Function			left
	@Input				iSize	number of characters to return (excluding null character)
	@Returns			The leftmost 'iSize' characters of the string.
	@Description		Returns the leftmost characters of the string (excluding 
	the null character) in a new CPVRTString. If iSize is
	larger than the string, a copy of the original string is returned.
	*************************************************************************/
	CPVRTString left(size_t iSize) const;

	/*!***********************************************************************
	@Function			right
	@Input				iSize	number of characters to return (excluding null character)
	@Returns			The rightmost 'iSize' characters of the string.
	@Description		Returns the rightmost characters of the string (excluding 
	the null character) in a new CPVRTString. If iSize is
	larger than the string, a copy of the original string is returned.
	*************************************************************************/
	CPVRTString right(size_t iSize) const;

	//allocator_type get_allocator( ) const;

	//CPVRTString& insert(size_t _P0, const char* _Ptr);
	//CPVRTString& insert(size_t _P0, const char* _Ptr, size_t _Count);
	//CPVRTString& insert(size_t _P0, const CPVRTString& _Str);
	//CPVRTString& insert(size_t _P0, const CPVRTString& _Str, size_t _Off, size_t _Count);
	//CPVRTString& insert(size_t _P0, size_t _Count, char _Ch);
	//iterator insert(iterator _It, char _Ch = char());
	//template<class InputIterator> void insert(iterator _It, InputIterator _First, InputIterator _Last);
	//void insert(iterator _It, size_t _Count, char _Ch);

	/*!***********************************************************************
	@Function			length
	@Returns			Length of the string
	@Description		Returns the length of the string
	*************************************************************************/
	size_t length() const;

	/*!***********************************************************************
	@Function			max_size
	@Returns			The maximum number of chars that the string can contain
	@Description		Returns the maximum number of chars that the string can contain
	*************************************************************************/
	size_t max_size() const;

	/*!***********************************************************************
	@Function			push_back
	@Input				_Ch A char to append
	@Description		Appends _Ch to the string
	*************************************************************************/
	void push_back(char _Ch);

	// const_reverse_iterator rbegin() const;
	// reverse_iterator rbegin();

	// const_reverse_iterator rend() const;
	// reverse_iterator rend();

	//CPVRTString& replace(size_t _Pos1, size_t _Num1, const char* _Ptr);
	//CPVRTString& replace(size_t _Pos1, size_t _Num1, const CPVRTString& _Str);
	//CPVRTString& replace(size_t _Pos1, size_t _Num1, const char* _Ptr, size_t _Num2);
	//CPVRTString& replace(size_t _Pos1, size_t _Num1, const CPVRTString& _Str, size_t _Pos2, size_t _Num2);
	//CPVRTString& replace(size_t _Pos1, size_t _Num1, size_t _Count, char _Ch);

	//CPVRTString& replace(iterator _First0, iterator _Last0, const char* _Ptr);
	//CPVRTString& replace(iterator _First0, iterator _Last0, const CPVRTString& _Str);
	//CPVRTString& replace(iterator _First0, iterator _Last0, const char* _Ptr, size_t _Num2);
	//CPVRTString& replace(iterator _First0, iterator _Last0, size_t _Num2, char _Ch);
	//template<class InputIterator> CPVRTString& replace(iterator _First0, iterator _Last0, InputIterator _First, InputIterator _Last);

	/*!***********************************************************************
	@Function			reserve
	@Input				_Count Size of string to reserve
	@Description		Reserves space for _Count number of chars
	*************************************************************************/
	void reserve(size_t _Count = 0);

	/*!***********************************************************************
	@Function			resize
	@Input				_Count 	Size of string to resize to
	@Input				_Ch		Character to use to fill any additional space
	@Description		Resizes the string to _Count in length
	*************************************************************************/
	void resize(size_t _Count, char _Ch = char());

	//size_t rfind(char _Ch, size_t _Off = npos) const;
	//size_t rfind(const char* _Ptr, size_t _Off = npos) const;
	//size_t rfind(const char* _Ptr, size_t _Off = npos, size_t _Count) const;
	//size_t rfind(const CPVRTString& _Str, size_t _Off = npos) const;

	/*!***********************************************************************
	@Function			size
	@Returns			Size of the string
	@Description		Returns the size of the string
	*************************************************************************/
	size_t size() const;

	/*!***********************************************************************
	@Function			substr
	@Input				_Off	Start of the substring
	@Input				_Count	Length of the substring
	@Returns			A substring of the string
	@Description		Returns the size of the string
	*************************************************************************/
	CPVRTString substr(size_t _Off = 0, size_t _Count = npos) const;

	/*!***********************************************************************
	@Function			swap
	@Input				_Str	A string to swap with
	@Description		Swaps the contents of the string with _Str
	*************************************************************************/
	void swap(CPVRTString& _Str);

	/*!***********************************************************************
	@Function			toLower
	@Returns			An updated string
	@Description		Converts the string to lower case
	*************************************************************************/
	CPVRTString& toLower();
	
	/*!***********************************************************************
	@Function			toUpper
	@Returns			An updated string
	@Description		Converts the string to upper case
	*************************************************************************/
	CPVRTString& toUpper();

	/*!***********************************************************************
	@Function			format
	@Input				pFormat A string containing the formating
	@Returns			A formatted string
	@Description		return the formatted string
	************************************************************************/
	CPVRTString format(const char *pFormat, ...);
	
	/*!***********************************************************************
	@Function			+=
	@Input				_Ch A char
	@Returns			An updated string
	@Description		+= Operator
	*************************************************************************/
	CPVRTString& operator+=(char _Ch);

	/*!***********************************************************************
	@Function			+=
	@Input				_Ptr A string
	@Returns			An updated string
	@Description		+= Operator
	*************************************************************************/
	CPVRTString& operator+=(const char* _Ptr);

	/*!***********************************************************************
	@Function			+=
	@Input				_Right A string
	@Returns			An updated string
	@Description		+= Operator
	*************************************************************************/
	CPVRTString& operator+=(const CPVRTString& _Right);

	/*!***********************************************************************
	@Function			=
	@Input				_Ch A char
	@Returns			An updated string
	@Description		= Operator
	*************************************************************************/
	CPVRTString& operator=(char _Ch);

	/*!***********************************************************************
	@Function			=
	@Input				_Ptr A string
	@Returns			An updated string
	@Description		= Operator
	*************************************************************************/
	CPVRTString& operator=(const char* _Ptr);

	/*!***********************************************************************
	@Function			=
	@Input				_Right A string
	@Returns			An updated string
	@Description		= Operator
	*************************************************************************/
	CPVRTString& operator=(const CPVRTString& _Right);

	/*!***********************************************************************
	@Function			[]
	@Input				_Off An index into the string
	@Returns			A character
	@Description		[] Operator
	*************************************************************************/
	const_reference operator[](size_t _Off) const;

	/*!***********************************************************************
	@Function			[]
	@Input				_Off An index into the string
	@Returns			A character
	@Description		[] Operator
	*************************************************************************/
	reference operator[](size_t _Off);

	/*!***********************************************************************
	@Function			+
	@Input				_Left A string
	@Input				_Right A string
	@Returns			An updated string
	@Description		+ Operator
	*************************************************************************/
	friend CPVRTString operator+ (const CPVRTString& _Left, const CPVRTString& _Right);

	/*!***********************************************************************
	@Function			+
	@Input				_Left A string
	@Input				_Right A string
	@Returns			An updated string
	@Description		+ Operator
	*************************************************************************/
	friend CPVRTString operator+ (const CPVRTString& _Left, const char* _Right);

	/*!***********************************************************************
	@Function			+
	@Input				_Left A string
	@Input				_Right A string
	@Returns			An updated string
	@Description		+ Operator
	*************************************************************************/
	friend CPVRTString operator+ (const CPVRTString& _Left, const char _Right);

	/*!***********************************************************************
	@Function			+
	@Input				_Left A string
	@Input				_Right A string
	@Returns			An updated string
	@Description		+ Operator
	*************************************************************************/
	friend CPVRTString operator+ (const char* _Left, const CPVRTString& _Right);


	/*!***********************************************************************
	@Function			+
	@Input				_Left A string
	@Input				_Right A string
	@Returns			An updated string
	@Description		+ Operator
	*************************************************************************/
	friend CPVRTString operator+ (const char _Left, const CPVRTString& _Right);

protected:
	char* m_pString;
	size_t m_Size;
	size_t m_Capacity;
};

/*************************************************************************
* MISCELLANEOUS UTILITY FUNCTIONS
*************************************************************************/
/*!***********************************************************************
@Function			PVRTStringGetFileExtension
@Input				strFilePath A string
@Returns			Extension
@Description		Extracts the file extension from a file path.
Returns an empty CPVRTString if no extension is found.
************************************************************************/
CPVRTString PVRTStringGetFileExtension(const CPVRTString& strFilePath);

/*!***********************************************************************
@Function			PVRTStringGetContainingDirectoryPath
@Input				strFilePath A string
@Returns			Directory
@Description		Extracts the directory portion from a file path.
************************************************************************/
CPVRTString PVRTStringGetContainingDirectoryPath(const CPVRTString& strFilePath);

/*!***********************************************************************
@Function			PVRTStringGetFileName
@Input				strFilePath A string
@Returns			FileName
@Description		Extracts the name and extension portion from a file path.
************************************************************************/
CPVRTString PVRTStringGetFileName(const CPVRTString& strFilePath);

/*!***********************************************************************
@Function			PVRTStringStripWhiteSpaceFromStartOf
@Input				strLine A string
@Returns			Result of the white space stripping
@Description		strips white space characters from the beginning of a CPVRTString.
************************************************************************/
CPVRTString PVRTStringStripWhiteSpaceFromStartOf(const CPVRTString& strLine);

/*!***********************************************************************
@Function			PVRTStringStripWhiteSpaceFromEndOf
@Input				strLine A string
@Returns			Result of the white space stripping
@Description		strips white space characters from the end of a CPVRTString.
************************************************************************/
CPVRTString PVRTStringStripWhiteSpaceFromEndOf(const CPVRTString& strLine);

/*!***********************************************************************
@Function			PVRTStringFromFormattedStr
@Input				pFormat A string containing the formating
@Returns			A formatted string
@Description		Creates a formatted string
************************************************************************/
CPVRTString PVRTStringFromFormattedStr(const char *pFormat, ...);

#endif // _PVRTSTRING_H_


/*****************************************************************************
End of file (PVRTString.h)
*****************************************************************************/




