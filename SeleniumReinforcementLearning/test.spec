
Test Case: "Click the 'Good to go' button"
	- Navigate to "file:///C:/sources/SeleniumReinforcementLearning/SeleniumReinforcementLearning/bin/Debug/test.html" // Opens the test case
	- Click {good-to-go} // Searches and clicks the good-to-go button (Already trained from Q)
	- Wait for {.third} to be visible // Wait command for verification

// What level of automation are we waiting to (or can we) have?
Test Case: "Messages: Create new message"
	- Navigate to "https://intraactiveamatest.sharepoint.com/sites/MessagesRLUITest"
	- Click "Add"
	- Type a link
	- Type a header
	- Type a text
	- Click "Save and close"
	- Search in messages that it is there

// Or
Test Case: "Messages: Create new message"
	- Navigate to "https://intraactiveamatest.sharepoint.com/sites/MessagesRLUITest"
	- Link: /a/b/c
	- Header: header
	- Message: message
	- Try to find a way to see it in the list