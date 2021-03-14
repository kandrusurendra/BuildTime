/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;

namespace VSBuildTimer
{
	/// <summary>
	/// This class is used to expose the list of the IDs of the commands implemented
	/// by the client package. This list of IDs must match the set of IDs defined inside the
	/// BUTTONS section of the CTC file.
	/// </summary>
	static class PkgCmdId
	{
		// Define the list a set of public static members.
        public const int cmdidBuildTimerWindow   = 0x2001; 

		// Define the list of menus (these include toolbars)
		public const int IDM_MyToolbar =           0x0101;
	}
}
