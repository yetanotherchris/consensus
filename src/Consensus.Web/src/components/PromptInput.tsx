import React, { useState } from 'react';
import { TextField, IconButton, Paper } from '@mui/material';
import SendIcon from '@mui/icons-material/Send';

interface PromptInputProps {
  onSubmit: (prompt: string) => void;
  disabled?: boolean;
}

export const PromptInput: React.FC<PromptInputProps> = ({ onSubmit, disabled = false }) => {
  const [prompt, setPrompt] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (prompt.trim() && !disabled) {
      onSubmit(prompt);
      setPrompt('');
    }
  };

  return (
    <Paper
      component="form"
      onSubmit={handleSubmit}
      elevation={3}
      sx={{
        p: 2,
        display: 'flex',
        alignItems: 'flex-end',
        gap: 1,
        borderRadius: 3,
      }}
    >
      <TextField
        fullWidth
        multiline
        maxRows={6}
        value={prompt}
        onChange={(e) => setPrompt(e.target.value)}
        placeholder="Enter your prompt here..."
        disabled={disabled}
        variant="standard"
        sx={{
          '& .MuiInputBase-root': {
            fontSize: '1rem',
          },
        }}
      />
      <IconButton
        type="submit"
        color="primary"
        disabled={!prompt.trim() || disabled}
        sx={{
          bgcolor: 'primary.main',
          color: 'white',
          '&:hover': {
            bgcolor: 'primary.dark',
          },
          '&.Mui-disabled': {
            bgcolor: 'action.disabledBackground',
          },
        }}
      >
        <SendIcon />
      </IconButton>
    </Paper>
  );
};
