import React, { useState } from 'react';
import { Box, IconButton, InputBase } from '@mui/material';
import ArrowUpwardIcon from '@mui/icons-material/ArrowUpward';

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

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSubmit(e);
    }
  };

  return (
    <Box
      component="form"
      onSubmit={handleSubmit}
      sx={{
        position: 'relative',
        width: '100%',
        maxWidth: '700px',
        margin: '0 auto',
      }}
    >
      <Box
        sx={{
          display: 'flex',
          alignItems: 'flex-end',
          backgroundColor: '#fff',
          borderRadius: '24px',
          border: '1px solid #e0e0e0',
          boxShadow: '0 2px 8px rgba(0,0,0,0.08)',
          padding: '12px 16px',
          transition: 'box-shadow 0.2s, border-color 0.2s',
          '&:focus-within': {
            borderColor: '#10a37f',
            boxShadow: '0 4px 12px rgba(16,163,127,0.15)',
          },
        }}
      >
        <InputBase
          multiline
          minRows={3}
          maxRows={8}
          value={prompt}
          onChange={(e) => setPrompt(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Enter your prompt here..."
          disabled={disabled}
          sx={{
            flex: 1,
            fontSize: '16px',
            lineHeight: '1.5',
            '& textarea': {
              resize: 'none',
            },
          }}
        />
        <IconButton
          type="submit"
          disabled={!prompt.trim() || disabled}
          sx={{
            marginLeft: 1,
            padding: '8px',
            backgroundColor: prompt.trim() && !disabled ? '#10a37f' : '#e0e0e0',
            color: '#fff',
            width: '36px',
            height: '36px',
            borderRadius: '50%',
            '&:hover': {
              backgroundColor: prompt.trim() && !disabled ? '#0d8a68' : '#e0e0e0',
            },
            '&.Mui-disabled': {
              backgroundColor: '#e0e0e0',
              color: '#999',
            },
            transition: 'background-color 0.2s',
          }}
        >
          <ArrowUpwardIcon sx={{ fontSize: '20px' }} />
        </IconButton>
      </Box>
    </Box>
  );
};
