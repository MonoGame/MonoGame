/******************************************************************************

 @File         PVRTArray.h

 @Title        PVRTArray

 @Version      

 @Copyright    Copyright (c) Imagination Technologies Limited. All Rights Reserved. Strictly Confidential.

 @Platform     ANSI compatible

 @Description  Expanding array template class. Allows appending and direct
               access. Mixing access methods should be approached with caution.

******************************************************************************/
#ifndef __PVRTARRAY_H__
#define __PVRTARRAY_H__

#include "PVRTGlobal.h"
#include "PVRTError.h"

/*!****************************************************************************
Class
******************************************************************************/

/*!***************************************************************************
* @Class CPVRTArray
* @Brief Expanding array template class.
* @Description Expanding array template class.
*****************************************************************************/
template<typename T>
class CPVRTArray
{
public:
	/*!***************************************************************************
	@Function			CPVRTArray
	@Description		Blank constructor. Makes a default sized array.
	*****************************************************************************/
	CPVRTArray() : m_uiSize(0), m_uiCapacity(GetDefaultSize())
	{
		m_pArray = new T[m_uiCapacity];
	}

	/*!***************************************************************************
	@Function			CPVRTArray
	@Input				uiSize	intial size of array
	@Description		Constructor taking initial size of array in elements.
	*****************************************************************************/
	CPVRTArray(const unsigned int uiSize) : m_uiSize(0), m_uiCapacity(uiSize)
	{
		_ASSERT(uiSize != 0);
		m_pArray = new T[uiSize];
	}

	/*!***************************************************************************
	@Function			CPVRTArray
	@Input				original	the other dynamic array
	@Description		Copy constructor.
	*****************************************************************************/
	CPVRTArray(const CPVRTArray& original) : m_uiSize(original.m_uiSize),
											  m_uiCapacity(original.m_uiCapacity)
	{
		m_pArray = new T[m_uiCapacity];
		for(unsigned int i=0;i<m_uiSize;i++)
		{
			m_pArray[i]=original.m_pArray[i];
		}
	}

	/*!***************************************************************************
	@Function			CPVRTArray
	@Input				pArray		an ordinary array
	@Input				uiSize		number of elements passed
	@Description		constructor from ordinary array.
	*****************************************************************************/
	CPVRTArray(const T* const pArray, const unsigned int uiSize) : m_uiSize(uiSize),
														  m_uiCapacity(uiSize)
	{
		_ASSERT(uiSize != 0);
		m_pArray = new T[uiSize];
		for(unsigned int i=0;i<m_uiSize;i++)
		{
			m_pArray[i]=pArray[i];
		}
	}

	/*!***************************************************************************
	@Function			CPVRTArray
	@Input				uiSize		initial capacity
	@Input				val			value to populate with
	@Description		constructor from a capacity and initial value.
	*****************************************************************************/
	CPVRTArray(const unsigned int uiSize, const T& val)	: m_uiSize(uiSize),
														m_uiCapacity(uiSize)
	{
		_ASSERT(uiSize != 0);
		m_pArray = new T[uiSize];
		for(unsigned int uiIndex = 0; uiIndex < m_uiSize; ++uiIndex)
		{
			m_pArray[uiIndex] = val;
		}
	}

	/*!***************************************************************************
	@Function			~CPVRTArray
	@Description		Destructor.
	*****************************************************************************/
	virtual ~CPVRTArray()
	{
		if(m_pArray)
			delete [] m_pArray;
	}

	/*!***************************************************************************
	@Function			Append
	@Input				addT	The element to append
	@Return				The index of the new item.
	@Description		Appends an element to the end of the array, expanding it
						if necessary.
	*****************************************************************************/
	unsigned int Append(const T& addT)
	{
		unsigned int uiIndex = Append();
		m_pArray[uiIndex] = addT;
		return uiIndex;
	}

	/*!***************************************************************************
	@Function			Append
	@Return				The index of the new item.
	@Description		Creates space for a new item, but doesn't add. Instead
						returns the index of the new item.
	*****************************************************************************/
	unsigned int Append()
	{
		unsigned int uiIndex = m_uiSize;
		SetCapacity(m_uiSize+1);
		m_uiSize++;

		return uiIndex;
	}

	/*!***************************************************************************
	@Function		Clear
	@Description	Clears the array.
	*****************************************************************************/
	void Clear()
	{
		m_uiSize = 0U;
	}

	/*!***************************************************************************
	@Function			SetSize
	@Input				uiSize		New capacity of array
	@Description		Expands array to new capacity
	*****************************************************************************/
	EPVRTError SetCapacity(const unsigned int uiSize)
	{
		if(uiSize <= m_uiCapacity)
			return PVR_SUCCESS;	// nothing to be done

		unsigned int uiNewCapacity;
		if(uiSize < m_uiCapacity*2)
		{
			uiNewCapacity = m_uiCapacity*2;			// Ignore the new size. Expand to twice the previous size.
		}
		else
		{
			uiNewCapacity = uiSize;
		}

		T* pNewArray = new T[uiNewCapacity];		// New Array
		if(!pNewArray)
			return PVR_FAIL;						// Failed to allocate memory!

		// Copy source data to new array
		for(unsigned int i = 0; i < m_uiSize; ++i)
		{
			pNewArray[i] = m_pArray[i];
		}

		// Switch pointers and free memory
		m_uiCapacity	= uiNewCapacity;
		T* pOldArray	= m_pArray;
		m_pArray		= pNewArray;
		delete [] pOldArray;
		return PVR_SUCCESS;
	}

	/*!***************************************************************************
	@Function			Copy
	@Input				other	The CPVRTArray needing copied
	@Description		A copy function. Will attempt to copy from other CPVRTArrays
						if this is possible.
	*****************************************************************************/
	template<typename T2>
	void Copy(const CPVRTArray<T2>& other)
	{
		T* pNewArray = new T[other.GetCapacity()];
		if(pNewArray)
		{
			// Copy data
			for(unsigned int i = 0; i < other.GetSize(); i++)
			{
				pNewArray[i] = other[i];
			}

			// Free current array
			if(m_pArray)
				delete [] m_pArray;

			// Swap pointers
			m_pArray		= pNewArray;

			m_uiCapacity	= other.GetCapacity();
			m_uiSize		= other.GetSize();
		}
	}

	/*!***************************************************************************
	@Function			=
	@Input				other	The CPVRTArray needing copied
	@Description		assignment operator.
	*****************************************************************************/
	CPVRTArray& operator=(const CPVRTArray<T>& other)
	{
		if(&other != this)
			Copy(other);

		return *this;
	}

	/*!***************************************************************************
	@Function		operator+=
	@Input			other		the array to append.
	@Description	appends an existing CPVRTArray on to this one.
	*****************************************************************************/
	CPVRTArray& operator+=(const CPVRTArray<T>& other)
	{
		if(&other != this)
		{
			for(unsigned int uiIndex = 0; uiIndex < other.GetSize(); ++uiIndex)
			{
				Append(other[uiIndex]);
			}
		}

		return *this;
	}

	/*!***************************************************************************
	@Function			[]
	@Input				uiIndex	index of element in array
	@Return				the element indexed
	@Description		indexed access into array. Note that this has no error
						checking whatsoever
	*****************************************************************************/
	T& operator[](const unsigned int uiIndex)
	{
		_ASSERT(uiIndex < m_uiCapacity);
		return m_pArray[uiIndex];
	}

	/*!***************************************************************************
	@Function			[]
	@Input				uiIndex	index of element in array
	@Return				The element indexed
	@Description		Indexed access into array. Note that this has no error
						checking whatsoever
	*****************************************************************************/
	const T& operator[](const unsigned int uiIndex) const
	{
		_ASSERT(uiIndex < m_uiCapacity);
		return m_pArray[uiIndex];
	}

	/*!***************************************************************************
	@Function			GetSize
	@Return				Size of array
	@Description		Gives current size of array/number of elements
	*****************************************************************************/
	unsigned int GetSize() const
	{
		return m_uiSize;
	}

	/*!***************************************************************************
	@Function			GetDefaultSize
	@Return				Default size of array
	@Description		Gives the default size of array/number of elements
	*****************************************************************************/
	static unsigned int GetDefaultSize()
	{
		return 16U;
	}

	/*!***************************************************************************
	@Function			GetCapacity
	@Return				Capacity of array
	@Description		Gives current allocated size of array/number of elements
	*****************************************************************************/
	unsigned int GetCapacity() const
	{
		return m_uiCapacity;
	}

	/*!***************************************************************************
	@Function			Contains
	@Input				object		The object to check in the array
	@Return				true if object is contained in this array.
	@Description		Indicates whether the given object resides inside the 
						array.
	*****************************************************************************/
	bool Contains(const T& object) const
	{
		for(unsigned int uiIndex = 0; uiIndex < m_uiSize; ++uiIndex)
		{
			if(m_pArray[uiIndex] == object)
				return true;
		}
		return false;
	}

	/*!***************************************************************************
	@Function			Sort
	@Input				predicate		The object which defines "bool operator()"
	@Description		Simple bubble-sort of the array. Pred should be an object that
						defines a bool operator().
	*****************************************************************************/
	template<class Pred>
	void Sort(Pred predicate)
	{
		bool bSwap;
		for(unsigned int i=0; i < m_uiSize; ++i)
		{
			bSwap = false;
			for(unsigned int j=0; j < m_uiSize-1; ++j)
			{
				if(predicate(m_pArray[j], m_pArray[j+1]))
				{
					PVRTswap(m_pArray[j], m_pArray[j+1]);
					bSwap = true;
				}
			}

			if(!bSwap)
				return;
		}
	}

	/*!***************************************************************************
	@Function		Remove
	@Input			uiIndex		The index to remove
	@Return			success or failure
	@Description	Removes an element from the array.
	*****************************************************************************/
	virtual EPVRTError Remove(unsigned int uiIndex)
	{
		_ASSERT(uiIndex < m_uiSize);
		if(m_uiSize == 0)
			return PVR_FAIL;

		if(uiIndex == m_uiSize-1)
		{
			return RemoveLast();
		}
        
        m_uiSize--;
        // Copy the data. memmove will only work for built-in types.
        for(unsigned int uiNewIdx = uiIndex; uiNewIdx < m_uiSize; ++uiNewIdx)
        {
            m_pArray[uiNewIdx] = m_pArray[uiNewIdx+1];
        }
		
		return PVR_SUCCESS;
	}

	/*!***************************************************************************
	@Function			RemoveLast
	@Return				success or failure
	@Description		Removes the last element. Simply decrements the size value
	*****************************************************************************/
	virtual EPVRTError RemoveLast()
	{
		if(m_uiSize > 0)
		{
			m_uiSize--;
			return PVR_SUCCESS;
		}	
		else
		{
			return PVR_FAIL;
		}
	}

protected:
	unsigned int 	m_uiSize;				/*! current size of contents of array */
	unsigned int	m_uiCapacity;			/*! currently allocated size of array */
	T				*m_pArray;				/*! the actual array itself */
};

// note "this" is required for ISO standard C++ and gcc complains otherwise
// http://lists.apple.com/archives/Xcode-users//2005/Dec/msg00644.html
template<typename T>
class CPVRTArrayManagedPointers : public CPVRTArray<T*>
{
public:
	virtual ~CPVRTArrayManagedPointers()
	{
		if(this->m_pArray)
		{
			for(unsigned int i=0;i<this->m_uiSize;i++)
			{
				delete(this->m_pArray[i]);
			}
		}
	}

	/*!***************************************************************************
	@Function		Remove
	@Input			uiIndex		The index to remove
	@Return			success or failure
	@Description	Removes an element from the array.
	*****************************************************************************/
	virtual EPVRTError Remove(unsigned int uiIndex)
	{
		_ASSERT(uiIndex < this->m_uiSize);
		if(this->m_uiSize == 0)
			return PVR_FAIL;

		if(uiIndex == this->m_uiSize-1)
		{
			return this->RemoveLast();
		}

		unsigned int uiSize = (this->m_uiSize - (uiIndex+1)) * sizeof(T*);
	
		delete this->m_pArray[uiIndex];
		memmove(this->m_pArray + uiIndex, this->m_pArray + (uiIndex+1), uiSize);

		this->m_uiSize--;
		return PVR_SUCCESS;
	}

	/*!***************************************************************************
	@Function			RemoveLast
	@Return				success or failure
	@Description		Removes the last element. Simply decrements the size value
	*****************************************************************************/
	virtual EPVRTError RemoveLast()
	{
		if(this->m_uiSize > 0 && this->m_pArray)
		{
			delete this->m_pArray[this->m_uiSize-1];
			this->m_uiSize--;
			return PVR_SUCCESS;
		}
		else
		{
			return PVR_FAIL;
		}
	}
};

#endif // __PVRTARRAY_H__

/*****************************************************************************
End of file (PVRTArray.h)
*****************************************************************************/

