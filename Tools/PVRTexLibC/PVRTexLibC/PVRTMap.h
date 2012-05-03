/******************************************************************************

 @File         PVRTMap.h

 @Title        PVRTArray

 @Version      

 @Copyright    Copyright (c) Imagination Technologies Limited. All Rights Reserved. Strictly Confidential.

 @Platform     ANSI compatible

 @Description  A simple and easy to use implementation of a map.

******************************************************************************/
#ifndef __PVRTMAP_H__
#define __PVRTMAP_H__

#include "PVRTArray.h"

/*!****************************************************************************
Class
******************************************************************************/

/*!***************************************************************************
* @Class		CPVRTMap
* @Brief		Expanding map template class.
* @Description	A simple and easy to use implementation of a map.
*****************************************************************************/
template <typename KeyType, typename DataType>
class CPVRTMap
{
public:

	/*!***********************************************************************
	 @Function		CPVRTMap
	 @Return		A new CPVRTMap.
	 @Description	Constructor for a CPVRTMap.
	*************************************************************************/
	CPVRTMap() : m_Keys(), m_Data(), m_uiSize(0)
	{
		//Initialise both arrays to a minimum capacity of 16.
		//PVRTuint32 uiInitialSize=16;
		//m_Keys.SetCapacity(uiInitialSize);
		//m_Data.SetCapacity(uiInitialSize);
	}

	/*!***********************************************************************
	 @Function		~CPVRTMap
	 @Description	Destructor for a CPVRTMap.
	*************************************************************************/
	~CPVRTMap()
	{
		//Clear the map, that's enough
		Clear();
	}

	EPVRTError Reserve(const PVRTuint32 uiSize)
	{
		//Sets the capacity of each member array to the requested size. The array used will only expand.
		//Returns the most serious error from either method.
		return PVRT_MAX(m_Keys.SetCapacity(uiSize),m_Data.SetCapacity(uiSize));
	}

	/*!***********************************************************************
	 @Function		GetSize
	 @Return		Number of meaningful members in the map.
	 @Description	Returns the number of meaningful members in the map.
	*************************************************************************/
	PVRTuint32 GetSize() const
	{
		//Return the size.
		return m_uiSize;
	}

	/*!***********************************************************************
	 @Function		GetIndexOf
	 @Input			key
	 @Return		The index value for a mapped item.
	 @Description	Gets the position of a particular key/data within the map.
					If the return value is exactly equal to the value of 
					GetSize() then the item has not been found.
	*************************************************************************/
	PVRTuint32 GetIndexOf(const KeyType key) const
	{
		//Loop through all the valid keys.
		for (PVRTuint32 i=0; i<m_uiSize; ++i)
		{
			//Check if a key matches.
			if (m_Keys[i]==key)
			{
				//If a matched key is found, return the position.
				return i;
			}
		}

		//If not found, return the number of meaningful members.
		return m_uiSize;
	}

	/*!***********************************************************************
	 @Function		GetDataAtIndex
	 @Input			uiIndex
	 @Return		Data type at the specified position.
	 @Description	Returns a pointer to the Data at a particular index. 
					If the index supplied is not valid, NULL is returned 
					instead. Deletion of data at this pointer will lead
					to undefined behaviour.
	*************************************************************************/
	const DataType* GetDataAtIndex(const PVRTuint32 uiIndex)
	{
		return &(m_Data[uiIndex]);
	}

	/*!***********************************************************************
	 @Function		operator[]
	 @Input			key
	 @Return		Data that is mapped to 'key'.
	 @Description	If a mapping already exists for 'key' then it will return 
					the associated data. If no mapping currently exists, a new 
					element is created in place.
	*************************************************************************/
	DataType& operator[] (const KeyType key)
	{
		//Get the index of the key.
		PVRTuint32 uiIndex = GetIndexOf(key);

		//Check the index is valid
		if (uiIndex != m_uiSize)
		{
			//Return mapped data if the index is valid.
			return m_Data[uiIndex];
		}
		else
		{
			//Append the key to the Keys array.
			m_Keys.Append(key);

			//Create a new DataType.
			DataType sNewData;

			//Append the new pointer to the Data array.
			m_Data.Append(sNewData);

			//Increment the size of meaningful data.
			++m_uiSize;

			//Return the contents of pNewData.
			return m_Data[m_Keys.GetSize()-1];
		}
	}

	/*!***********************************************************************
	 @Function		Remove
	 @Input			key
	 @Return		Returns PVR_FAIL if item doesn't exist. 
					Otherwise returns PVR_SUCCESS.
	 @Description	Removes an element from the map if it exists.
	*************************************************************************/
	EPVRTError Remove(const KeyType key)
	{
		//Finds the index of the key.
		PVRTuint32 uiIndex=GetIndexOf(key);

		//If the key is invalid, fail.
		if (key==m_uiSize)
		{
			//Return failure.
			return PVR_FAIL;
		}
		
		//Decrement the size of the map to ignore the last element in each array.
		m_uiSize--;

		//Copy the last key over the deleted key. There are now two copies of one element, 
		//but the one at the end of the array is ignored.
		m_Keys[uiIndex]=m_Keys[m_uiSize-1];

		//Copy the last data over the deleted data in the same way as the keys.
		m_Data[uiIndex]=m_Data[m_uiSize-1];

		//Return success.
		return PVR_SUCCESS;
	}

	/*!***********************************************************************
	 @Function		Clear
	 @Description	Clears the Map of all data values.
	*************************************************************************/
	void Clear()
	{
		//Set the size to 0.
		m_uiSize=0;
	}

	/*!***********************************************************************
	 @Function		Exists
	 @Input			key
	 @Return		Whether data exists for the specified key or not.
	 @Description	Checks whether or not data exists for the specified key.
	*************************************************************************/
	bool Exists(const KeyType key) const
	{
		//Checks for a valid index for key, if not, returns false.
		return (GetIndexOf(key) != m_uiSize);
	}

private:

	//Array of all the keys. Indices match m_Data.
	CPVRTArray<KeyType> m_Keys;

	//Array of pointers to all the allocated data.
	CPVRTArray<DataType> m_Data;

	//The number of meaningful members in the map.
	PVRTuint32 m_uiSize;
};

#endif // __PVRTMAP_H__

/*****************************************************************************
End of file (PVRTMap.h)
*****************************************************************************/

