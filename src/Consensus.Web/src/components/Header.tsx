import { Box, Typography } from '@mui/material';

interface HeaderProps {
  align?: 'left' | 'center';
  noMargin?: boolean;
}

export const Header: React.FC<HeaderProps> = ({ align = 'left', noMargin = false }) => {
  return (
    <Box sx={{ mb: noMargin ? 0 : 3, textAlign: align }}>
      <Typography 
        variant="h3" 
        component="h1" 
        sx={{ 
          fontWeight: 200, 
          fontFamily: '"Inter", "Segoe UI", "Roboto", sans-serif',
          margin: noMargin ? 0 : undefined,
          lineHeight: noMargin ? 1 : undefined,
        }}
      >
        consensus
      </Typography>
    </Box>
  );
};
