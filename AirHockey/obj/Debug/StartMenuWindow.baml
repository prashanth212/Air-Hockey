   M S B A M L   `   `   `  ����  	AirHockey    AirHockey.StartMenuWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  NetWork Air Hockey Game��5   	   $	��400��6)   $	��420��66    � 
ResizeMode$ CanMinimize=�6B    � WindowStartupLocation$ CenterScreen=�6[    � 	Resources 5       �� 
TargetType �� Property �� Value��%�  '��       �� 5   
   " ��6   .�5      ��  � ��$ Center=�64   6E   �� 5       � ��$ Center=�63   6D   �� 5	       � ��$ Calibri)�6+   6=   �� 5
       � ��$ 24(�6)   66   �� 5       � ��$ Black"�6+   6;   �� 5       � ��
 ��  �6>   �� 5       � ��$
 Wrap=�6-   6<   �� 5       �@��TextAlignment$ Center=�6.   6@   5      5      .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      6   .+�5   
   �� `^Welcome to the Network AirHockey Game! Fill all necessary fields and start playing. Good Luck!6t   �� 5   
   $��1��6   $��14(�6!   $��	0,10,0,10q�6/   (&Choose any available IP to use in game6j   �� 5   
   -   5        $��IPList��5   P   $��2��6   $	��120��6    $��30��6,   $	��Top=�68   6`   �� 5   
   -   5        $��ServerIPlabel��5   B   $��3��6   $��14(�6!   $��	0,10,0,10q�6/   $y�	Collapsed=�6W   Please enter server IP6�   �� 5    
   -   5        $��ServerIpBox��5    �   $��4��6   $	��120��6   $��30��6+    �� TextAlignment$ Center=�67   $��Calibri)�6N   $y�	Collapsed=�6d   $��16(�6{   $
��Bold"�6�   6�   �� 5!   
   	 �� Orientation$	 
Horizontal=�6   $��5��6.   .+�5"      � $��	10,0,10,0q�6    5#      �� 5$      6&   �� 5%      6&   .+�5'      	� -   5        $��IsServerRadio��5'   0   
 d� 	IsChecked$

 TrueY�6   $��Center=�6E   .��6�   Create Game6�   	� 5(      -   5        $��1��5(      $��Center=�6+   $
��Left=�6F   .��6   Connect to the Game6�   5)      �� 5*      -   5        $��StartButton��5*   8   $��80��6   $��10q�6    $��40��6,   .��6e   
Start Game6q   �� 5+      -   5        $��
ExitButton��5+   A   $��80��6   $��40,10,20,10q�6    $��40��65   .��6l   Exit6r   5,      5-      5.      