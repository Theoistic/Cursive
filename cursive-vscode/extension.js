// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
const vscode = require('vscode');
const fs = require('fs');
const path = require('path');


/**
 * @param {vscode.ExtensionContext} context
 */
function activate(context) {

	let p = context.path;

	context.subscriptions.push(vscode.commands.registerCommand('extension.cursive.cursiveexec', function(item) {
		var term = vscode.window.activeTerminal;
		if(term == null) {
			term = vscode.window.createTerminal();
		}
		term.show();
		term.sendText("cursive dbg -project \"" + vscode.workspace.rootPath + "\"");
	}));

	// The command has been defined in the package.json file
	// Now provide the implementation of the command with  registerCommand
	// The commandId parameter must match the command field in package.json
	let disposable = vscode.commands.registerCommand('extension.cursive.activated', function () {
		// The code you place here will be executed every time your command is executed

	});

	/*context.subscriptions.push(vscode.commands.registerCommand('extension.cursive.create', config => {
		return "file location is not figured out yet. lol";
	}));*/

	context.subscriptions.push(disposable);
}
exports.activate = activate;

// this method is called when your extension is deactivated
function deactivate() {}

module.exports = {
	activate,
	deactivate
}
